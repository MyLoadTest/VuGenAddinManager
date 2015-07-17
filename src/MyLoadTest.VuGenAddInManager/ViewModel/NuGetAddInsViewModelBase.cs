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
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    /// <summary>
    /// Base class for view models displaying NuGet
    /// </summary>
    public abstract class NuGetAddInsViewModelBase : AddInsViewModelBase
    {
        private AddInManagerTask<ReadPackagesResult> _task;
        private IEnumerable<IPackage> _allPackages;

        public NuGetAddInsViewModelBase()
        {
        }

        public NuGetAddInsViewModelBase(IAddInManagerServices services)
            : base(services)
        {
        }

        public override int SelectedPageNumber
        {
            get
            {
                return base.SelectedPageNumber;
            }

            set
            {
                if (base.SelectedPageNumber != value)
                {
                    base.SelectedPageNumber = value;
                    StartReadPackagesTask();
                    OnPropertyChanged(null);
                }
            }
        }

        public override void ReadPackages()
        {
            base.ReadPackages();
            _allPackages = null;
            UpdateRepositoryBeforeReadPackagesTaskStarts();
            StartReadPackagesTask();
        }

        /// <summary>
        /// Returns all the packages.
        /// </summary>
        protected virtual IQueryable<IPackage> GetAllPackages()
        {
            return null;
        }

        protected virtual void UpdateRepositoryBeforeReadPackagesTaskStarts()
        {
        }

        protected IQueryable<IPackage> FilterPackagesBySearchCriteria(IQueryable<IPackage> packages, string searchCriteria)
        {
            return packages.Find(searchCriteria);
        }

        /// <summary>
        /// Allows filtering of the packages before paging the results. Call base class method
        /// to run default filtering.
        /// </summary>
        protected virtual IEnumerable<IPackage> GetFilteredPackagesBeforePagingResults(IQueryable<IPackage> allPackages)
        {
            return GetBufferedPackages(allPackages)
                .Where(package => ShowPrereleases || package.IsReleaseVersion())
                .DistinctLast<IPackage>(PackageEqualityComparer.Id);
        }

        protected virtual void UpdatePackageViewModels(IEnumerable<IPackage> packages)
        {
            var currentViewModels = ConvertToAddInViewModels(packages);
            UpdatePackageViewModels(currentViewModels);
        }

        protected IEnumerable<AddInPackageViewModelBase> ConvertToAddInViewModels(IEnumerable<IPackage> packages)
        {
            return packages.Select(CreateAddInViewModel);
        }

        protected virtual AddInPackageViewModelBase CreateAddInViewModel(IPackage package)
        {
            return new NuGetPackageViewModel(AddInManager, package);
        }

        private static IEnumerable<IPackage> GetBufferedPackages(IQueryable<IPackage> allPackages)
        {
            return allPackages.AsBufferedEnumerable(30);
        }

        private static IQueryable<IPackage> OrderPackages(IQueryable<IPackage> packages)
        {
            return packages
                .OrderBy(package => package.Id)
                .ThenBy(package => package.Version);
        }

        private void StartReadPackagesTask()
        {
            HasError = false;
            IsReadingPackages = true;

            // ClearPackages();
            CancelReadPackagesTask();
            CreateReadPackagesTask();
            _task.Start();

            // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Started task");
        }

        private void CancelReadPackagesTask()
        {
            if (_task != null)
            {
                // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Cancelled task");
                _task.Cancel();
            }
        }

        private void CreateReadPackagesTask()
        {
            _task = AddInManagerTask.Create(
                GetPackagesForSelectedPageResult,
                OnPackagesReadForSelectedPage);

            // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Created task");
        }

        private ReadPackagesResult GetPackagesForSelectedPageResult()
        {
            var packages = GetPackagesForSelectedPage();
            return new ReadPackagesResult(packages.ToArray(), TotalItems);
        }

        private void OnPackagesReadForSelectedPage(AddInManagerTask<ReadPackagesResult> task)
        {
            // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Task has returned");
            IsReadingPackages = false;
            var wasSuccessful = false;
            var wasCancelled = false;
            if (task.IsFaulted)
            {
                ClearPackages();
                SaveError(task.Exception);
            }
            else if (task.IsCancelled)
            {
                // Ignore
                // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Task ignored, because cancelled");
                wasCancelled = true;
            }
            else
            {
                // SD.Log.Debug("[AddInManager2] NuGetAddInsViewModelBase: Task successfully finished.");
                UpdatePackagesForSelectedPage(task.Result);
                wasSuccessful = true;
            }

            OnPropertyChanged(null);
            AddInManager.Events.OnPackageListDownloadEnded(this, new PackageListDownloadEndedEventArgs(wasSuccessful, wasCancelled));
        }

        private void UpdatePackagesForSelectedPage(ReadPackagesResult result)
        {
            PagesCollection.TotalItems = result.TotalPackages;
            PagesCollection.TotalItemsOnSelectedPage = result.TotalPackagesOnPage;
            UpdatePackageViewModels(result.Packages);
        }

        private IEnumerable<IPackage> GetPackagesForSelectedPage()
        {
            var filteredPackages = GetFilteredPackagesBeforePagingResults();
            return GetPackagesForSelectedPage(filteredPackages);
        }

        private IEnumerable<IPackage> GetFilteredPackagesBeforePagingResults()
        {
            if (_allPackages == null)
            {
                var packages = GetAllPackages();
                packages = OrderPackages(packages);
                packages = FilterPackagesBySearchCriteria(packages);
                TotalItems = packages.Count();
                _allPackages = GetFilteredPackagesBeforePagingResults(packages);
            }

            return _allPackages;
        }

        private IQueryable<IPackage> FilterPackagesBySearchCriteria(IQueryable<IPackage> packages)
        {
            var searchCriteria = GetSearchCriteria();
            return FilterPackagesBySearchCriteria(packages, searchCriteria);
        }

        private string GetSearchCriteria()
        {
            if (string.IsNullOrWhiteSpace(SearchTerms))
            {
                return null;
            }

            return SearchTerms;
        }

        private IEnumerable<IPackage> GetPackagesForSelectedPage(IEnumerable<IPackage> allPackages)
        {
            var packagesToSkip = PagesCollection.ItemsBeforeFirstPage;
            return allPackages
                .Skip(packagesToSkip)
                .Take(PagesCollection.PageSize);
        }
    }
}