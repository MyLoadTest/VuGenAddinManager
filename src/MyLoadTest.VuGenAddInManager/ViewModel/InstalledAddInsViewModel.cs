// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using MyLoadTest.VuGenAddInManager.Properties;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    /// <summary>
    /// Model for view of installed SharpDevelop AddIns.
    /// </summary>
    public sealed class InstalledAddInsViewModel : NuGetAddInsViewModelBase
    {
        public InstalledAddInsViewModel()
        {
            Initialize();
        }

        public InstalledAddInsViewModel(IAddInManagerServices services)
            : base(services)
        {
            Initialize();
        }

        protected override void OnDispose()
        {
            AddInManager.Events.AddInInstalled -= InstalledAddInsChanged;
            AddInManager.Events.AddInUninstalled -= InstalledAddInsChanged;
            AddInManager.Events.AddInStateChanged -= InstalledAddInStateChanged;
        }

        protected override IQueryable<IPackage> GetAllPackages()
        {
            return AddInManager.NuGet.Packages.LocalRepository.GetPackages();
        }

        protected override IEnumerable<IPackage> GetFilteredPackagesBeforePagingResults(
            IQueryable<IPackage> allPackages)
        {
            return base.GetFilteredPackagesBeforePagingResults(allPackages)
                .Where(package => package.IsReleaseVersion())
                .DistinctLast<IPackage>(PackageEqualityComparer.Id);
        }

        protected override void UpdatePackageViewModels(IEnumerable<IPackage> packages)
        {
            var offlineAddInViewModels = GetInstalledAddIns(packages);
            UpdatePackageViewModels(offlineAddInViewModels.OrderBy(vm => vm.Name));
        }

        protected override void UpdatePreinstalledFilter()
        {
            // Save the preinstalled AddIn filter
            SavePreinstalledAddInFilter();

            // Update the list
            Search();
        }

        protected override void InstallFromArchive()
        {
            // Notify about new operation
            AddInManager.Events.OnOperationStarted();

            var dlg = new OpenFileDialog
            {
                Filter = Resources.AddInManager2_SDAddInFileFilter,
                Multiselect = true
            };

            var showDialogResult = dlg.ShowDialog();
            if (showDialogResult ?? false)
            {
                foreach (var file in dlg.FileNames)
                {
                    AddInManager.Setup.InstallAddIn(file);
                }
            }
        }

        private void Initialize()
        {
            AllowInstallFromArchive = true;
            HasFilterForPreinstalled = true;
            Title = Resources.AddInManager2_Views_Installed;

            // Load preinstalled AddIn filter
            LoadPreinstalledAddInFilter();

            AddInManager.Events.AddInInstalled += InstalledAddInsChanged;
            AddInManager.Events.AddInUninstalled += InstalledAddInsChanged;
            AddInManager.Events.AddInStateChanged += InstalledAddInStateChanged;
        }

        private IEnumerable<AddInPackageViewModelBase> CombineOnlineAndOfflineAddIns(
            IEnumerable<AddInPackageViewModelBase> onlineAddIns,
            IEnumerable<AddInPackageViewModelBase> offlineAddIns)
        {
            return offlineAddIns.GroupJoin(
                onlineAddIns,
                offlinevm => offlinevm.Id,
                onlinevm => onlinevm.Id,
                (offlinevm, e) => e.ElementAtOrDefault(0) ?? offlinevm);
        }

        private IEnumerable<AddInPackageViewModelBase> GetInstalledAddIns(IEnumerable<IPackage> installedPackages)
        {
            // Fill set of ID of installed NuGet packages, so we can later quickly check, whether NuGet package is installed for an AddIn
            var packageIds = new HashSet<string>();
            foreach (var package in installedPackages)
            {
                if (!packageIds.Contains(package.Id))
                {
                    packageIds.Add(package.Id);
                }
            }

            var addInList = new List<ManagedAddIn>(AddInManager.Setup.AddInsWithMarkedForInstallation);
            addInList.Sort((a, b) => string.Compare(a.AddIn.Name, b.AddIn.Name, StringComparison.Ordinal));
            foreach (var addIn in addInList)
            {
                if (string.Equals(
                    addIn.AddIn.Properties["addInManagerHidden"],
                    "true",
                    StringComparison.OrdinalIgnoreCase))
                {
                    // This excludes the SharpDevelop application appearing as AddIn in the tree
                    continue;
                }

                if (!ShowPreinstalledAddIns && AddInManager.Setup.IsAddInPreinstalled(addIn.AddIn))
                {
                    continue;
                }

                var packageId = addIn.LinkedNuGetPackageID;
                if (!string.IsNullOrEmpty(packageId))
                {
                    if (packageIds.Contains(packageId))
                    {
                        addIn.InstallationSource = AddInInstallationSource.NuGetRepository;
                    }
                }

                AddInPackageViewModelBase addInPackage = new OfflineAddInsViewModel(AddInManager, addIn);
                yield return addInPackage;
            }
        }

        private void InstalledAddInsChanged(object sender, AddInInstallationEventArgs e)
        {
            ReadPackages();
        }

        private void InstalledAddInStateChanged(object sender, AddInInstallationEventArgs e)
        {
            UpdateInstallationState();
        }

        private void LoadPreinstalledAddInFilter()
        {
            ShowPreinstalledAddIns = AddInManager.Settings.ShowPreinstalledAddIns;
        }

        private void SavePreinstalledAddInFilter()
        {
            AddInManager.Settings.ShowPreinstalledAddIns = ShowPreinstalledAddIns;
        }
    }
}