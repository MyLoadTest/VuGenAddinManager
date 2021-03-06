﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using ICSharpCode.Core;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using MyLoadTest.VuGenAddInManager.Properties;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    /// <summary>
    /// View model representing a NuGet package in gallery.
    /// </summary>
    public sealed class NuGetPackageViewModel : AddInPackageViewModelBase
    {
        private readonly IPackage _package;
        private IEnumerable<PackageOperation> _packageOperations = new PackageOperation[0];

        public NuGetPackageViewModel(IPackage package)
        {
            _package = package;
        }

        public NuGetPackageViewModel(IAddInManagerServices services, IPackage package)
            : base(services)
        {
            _package = package;
        }

        public IPackage Package
        {
            get
            {
                return _package;
            }
        }

        public override string Name
        {
            get
            {
                return _package.Id;
            }
        }

        public override Uri LicenseUrl
        {
            get
            {
                return _package.LicenseUrl;
            }
        }

        public override Uri ProjectUrl
        {
            get
            {
                return _package.ProjectUrl;
            }
        }

        public override Uri ReportAbuseUrl
        {
            get
            {
                return _package.ReportAbuseUrl;
            }
        }

        public override bool IsOffline
        {
            get
            {
                return false;
            }
        }

        public override bool IsPreinstalled
        {
            get
            {
                return false;
            }
        }

        public override bool IsAdded
        {
            get
            {
                var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package, true);

                // if (installedAddIn != null)
                //  LoggingService.DebugFormatted("isAdded: installedAddIn.Action = {0}", installedAddIn.Action);
                // else
                //  LoggingService.DebugFormatted("isAdded: installedAddIn for {0} is null", _package.Id);
                return (installedAddIn != null)
                    && ((installedAddIn.Action == AddInAction.Install)
                        || (installedAddIn.Action == AddInAction.Update));
            }
        }

        public override bool IsUpdate
        {
            get
            {
                var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);

                // if (installedAddIn != null)
                //   LoggingService.DebugFormatted("isUpdate: installed {0}, package {1}", installedAddIn.Version.ToString(), _package.Version.Version.ToString());
                // else
                //   LoggingService.DebugFormatted("isUpdate: installedAddIn for {0} is null", _package.Id);
                return (installedAddIn != null)
                    && AddInManager.Setup.IsAddInInstalled(installedAddIn)
                    && (AddInManager.Setup.CompareAddInToPackageVersion(installedAddIn, _package) < 0);
            }
        }

        public override bool IsInstalled
        {
            get
            {
                var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);
                return (installedAddIn != null) && AddInManager.Setup.IsAddInInstalled(installedAddIn);
            }
        }

        public override bool IsInstallable
        {
            get
            {
                return true;
            }
        }

        public override bool IsUninstallable
        {
            get
            {
                return true;
            }
        }

        public override bool IsDisablingPossible
        {
            get
            {
                return false;
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        public override bool IsRemoved
        {
            get
            {
                var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);
                return (installedAddIn != null) && (installedAddIn.Action == AddInAction.Uninstall);
            }
        }

        public override IEnumerable<AddInDependency> Dependencies
        {
            get
            {
                if ((_package.DependencySets != null) && _package.DependencySets.Any())
                {
                    var firstSet = _package.DependencySets.First();
                    if ((firstSet != null) && (firstSet.Dependencies != null) && (firstSet.Dependencies.Count > 0))
                    {
                        return firstSet.Dependencies.Select(d => new AddInDependency(d));
                    }
                }

                return null;
            }
        }

        public override IEnumerable<string> Authors
        {
            get
            {
                return _package.Authors;
            }
        }

        public override bool HasDownloadCount
        {
            get
            {
                return _package.DownloadCount >= 0;
            }
        }

        public override string Id
        {
            get
            {
                return _package.Id;
            }
        }

        public override Uri IconUrl
        {
            get
            {
                return _package.IconUrl;
            }
        }

        public override string Summary
        {
            get
            {
                if (IsAdded)
                {
                    return IsUpdate
                        ? Resources.AddInManager_AddInUpdated
                        : SurroundWithParantheses(Resources.AddInManager_AddInInstalled);
                }

                if (IsRemoved)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInRemoved);
                }

                if (!IsEnabled)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInDisabled);
                }

                return _package.Summary;
            }
        }

        public override Version Version
        {
            get
            {
                return _package.Version != null ? _package.Version.ToVersion() : null;
            }
        }

        public override Version OldVersion
        {
            get
            {
                var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);
                return (installedAddIn != null) && IsUpdate
                    ? (installedAddIn.Properties.Contains(ManagedAddIn.NuGetPackageVersionManifestAttribute)
                        ? new Version(installedAddIn.Properties[ManagedAddIn.NuGetPackageVersionManifestAttribute])
                        : installedAddIn.Version)
                    : null;
            }
        }

        public override bool ShowSplittedVersions
        {
            get
            {
                return IsUpdate;
            }
        }

        public override int DownloadCount
        {
            get
            {
                return _package.DownloadCount;
            }
        }

        public override string Description
        {
            get
            {
                return _package.Description;
            }
        }

        public override DateTime? LastUpdated
        {
            get
            {
                // TODO
                // return package.LastUpdated;
                return null;
            }
        }

        public override bool HasDependencyConflicts
        {
            get
            {
                return false;
            }
        }

        public override void AddPackage()
        {
            ClearReportedMessages();
            TryInstallingPackage();
        }

        public override void UpdatePackage()
        {
            ClearReportedMessages();
            TryInstallingPackage();
        }

        public override void RemovePackage()
        {
            ClearReportedMessages();
            TryUninstallingPackage();
        }

        public void UninstallPackage()
        {
            ClearReportedMessages();

            var installedAddIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);
            if (installedAddIn != null)
            {
                AddInManager.Setup.UninstallAddIn(installedAddIn);
            }
        }

        public override void CancelInstallation()
        {
            ClearReportedMessages();

            var addIn = AddInManager.Setup.GetAddInForNuGetPackage(_package, true);
            if (addIn != null)
            {
                AddInManager.Setup.CancelInstallation(addIn);
            }
        }

        public override void CancelUpdate()
        {
            ClearReportedMessages();

            var addIn = AddInManager.Setup.GetAddInForNuGetPackage(_package, true);
            if (addIn != null)
            {
                AddInManager.Setup.CancelUpdate(addIn);
            }
        }

        public override void CancelUninstallation()
        {
            ClearReportedMessages();

            var addIn = AddInManager.Setup.GetAddInForNuGetPackage(_package);
            if (addIn != null)
            {
                AddInManager.Setup.CancelUninstallation(addIn);
            }
        }

        private bool IsPackageInstalled()
        {
            return AddInManager.NuGet.Packages.LocalRepository.Exists(_package);
        }

        private void ClearReportedMessages()
        {
            // Notify about new operation
            AddInManager.Events.OnOperationStarted();
        }

        private void GetPackageOperations()
        {
            var packageOperationResolver = AddInManager.NuGet.CreateInstallPackageOperationResolver(false);
            _packageOperations = packageOperationResolver.ResolveOperations(_package);
        }

        private bool CanInstallPackage()
        {
            // Ask for downloading dependent packages
            if ((_packageOperations != null) && _packageOperations.Any())
            {
                var operationsForDependencies = _packageOperations.Where(p => p.Package.Id != _package.Id).ToArray();
                if (operationsForDependencies.Any())
                {
                    var addInNames = string.Empty;
                    foreach (var packageOperation in operationsForDependencies)
                    {
                        addInNames += "\t " +
                            packageOperation.Package.Id + " " + packageOperation.Package.Version + Environment.NewLine;
                    }

                    if (!MessageService.AskQuestionFormatted(
                        "${res:AddInManager.Title}",
                        "${res:AddInManager2.InstallDependentMessage}",
                        _package.Id,
                        addInNames))
                    {
                        return false;
                    }
                }
            }

            // Ask for license acceptance
            var packages = GetPackagesRequiringLicenseAcceptance();
            if (packages.Any())
            {
                var acceptLicenses = new AcceptLicensesEventArgs(packages) { IsAccepted = true };
                AddInManager.Events.OnAcceptLicenses(acceptLicenses);
                return acceptLicenses.IsAccepted;
            }

            return true;
        }

        private IPackage[] GetPackagesRequiringLicenseAcceptance()
        {
            var packagesToBeInstalled = GetPackagesToBeInstalled();
            return GetPackagesRequiringLicenseAcceptance(packagesToBeInstalled);
        }

        private IPackage[] GetPackagesRequiringLicenseAcceptance(IEnumerable<IPackage> packagesToBeInstalled)
        {
            return packagesToBeInstalled.Where(PackageRequiresLicenseAcceptance).ToArray();
        }

        private IEnumerable<IPackage> GetPackagesToBeInstalled()
        {
            var packages = new List<IPackage>();
            foreach (var operation in _packageOperations)
            {
                if (operation.Action == PackageAction.Install)
                {
                    packages.Add(operation.Package);
                }
            }

            return packages;
        }

        private bool PackageRequiresLicenseAcceptance(IPackage package)
        {
            return package.RequireLicenseAcceptance && !IsPackageInstalled();
        }

        private void TryInstallingPackage()
        {
            try
            {
                if (IsPackageInstalled())
                {
                    // Package is already installed, but seems to be not registered as SD AddIn
                    AddInManager.Setup.InstallAddIn(_package, AddInManager.NuGet.GetLocalPackageDirectory(_package));
                }
                else
                {
                    // Perform a normal download and AddIn installation
                    GetPackageOperations();
                    if (CanInstallPackage())
                    {
                        InstallPackage(_packageOperations);
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
                LoggingService.Error("Error when trying to install package.", ex);
            }
        }

        private void InstallPackage(IEnumerable<PackageOperation> packageOperations)
        {
            foreach (var operation in packageOperations)
            {
                AddInManager.NuGet.ExecuteOperation(operation);
            }
        }

        private void ReportError(Exception ex)
        {
            AddInManager.Events.OnAddInOperationError(new AddInOperationErrorEventArgs(ex));
        }

        private void TryUninstallingPackage()
        {
            try
            {
                UninstallPackage();
            }
            catch (Exception ex)
            {
                ReportError(ex);
                LoggingService.Error("Error when trying to uninstall package.", ex);
            }
        }
    }
}