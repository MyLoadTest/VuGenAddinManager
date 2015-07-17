﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    public sealed class AvailableAddInsViewModel : NuGetAddInsViewModelBase
    {
        public AvailableAddInsViewModel()
        {
            Initialize();
        }

        public AvailableAddInsViewModel(IAddInManagerServices services)
            : base(services)
        {
            Initialize();
        }

        protected override void OnDispose()
        {
            AddInManager.Events.AddInInstalled -= AddInInstallationStateChanged;
            AddInManager.Events.AddInUninstalled -= AddInInstallationStateChanged;
            AddInManager.Events.AddInStateChanged += AddInInstallationStateChanged;
        }

        protected override IQueryable<IPackage> GetAllPackages()
        {
            return (ActiveRepository ?? AddInManager.Repositories.AllRegistered).GetPackages()
                .Where(package => package.IsLatestVersion);
        }

        protected override IEnumerable<IPackage> GetFilteredPackagesBeforePagingResults(IQueryable<IPackage> allPackages)
        {
            return base.GetFilteredPackagesBeforePagingResults(allPackages)
                .OrderByDescending(package => package.DownloadCount)
                .ThenBy(package => package.Id);
        }

        protected override void UpdatePrereleaseFilter()
        {
            ReadPackages();
        }

        private void Initialize()
        {
            IsSearchable = true;
            ShowPackageSources = true;
            Title = SD.ResourceService.GetString("AddInManager2.Views.Available");

            AddInManager.Events.AddInInstalled += AddInInstallationStateChanged;
            AddInManager.Events.AddInUninstalled += AddInInstallationStateChanged;
            AddInManager.Events.AddInStateChanged += AddInInstallationStateChanged;
        }

        private void AddInInstallationStateChanged(object sender, AddInInstallationEventArgs e)
        {
            UpdateInstallationState();
        }
    }
}