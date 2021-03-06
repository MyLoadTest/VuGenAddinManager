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
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using MyLoadTest.VuGenAddInManager.Properties;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.Model
{
    /// <summary>
    /// Holds reference to currently used NuGet package repositories to install AddIns from.
    /// </summary>
    public sealed class PackageRepositories : IPackageRepositories
    {
        public const string DefaultRepositoryName = "VuGen Add-in Repository";
        public const string DefaultRepositorySource = "https://addins.myloadtest.com/";

        private readonly List<PackageSource> _registeredPackageSources;
        private readonly IAddInManagerEvents _events;
        private readonly IAddInManagerSettings _settings;

        private IPackageRepository _aggregatedRepository;
        private IEnumerable<IPackageRepository> _registeredPackageRepositories;

        public PackageRepositories(IAddInManagerEvents events, IAddInManagerSettings settings)
        {
            _events = events;
            _settings = settings;

            _registeredPackageSources = new List<PackageSource>();

            LoadPackageSources();
            UpdateCurrentRepository();
        }

        public IPackageRepository AllRegistered
        {
            get
            {
                return _aggregatedRepository;
            }
        }

        public IEnumerable<PackageSource> RegisteredPackageSources
        {
            get
            {
                return _registeredPackageSources;
            }

            set
            {
                _registeredPackageSources.Clear();
                if (value != null)
                {
                    _registeredPackageSources.AddRange(value);
                }

                SavePackageSources();

                // Send around the update
                _events.OnPackageSourcesChanged(EventArgs.Empty);
            }
        }

        public IEnumerable<IPackageRepository> RegisteredPackageRepositories
        {
            get
            {
                return _registeredPackageRepositories;
            }
        }

        public IPackageRepository GetRepositoryFromSource(PackageSource packageSource)
        {
            IPackageRepository resultRepository;
            if (packageSource != null)
            {
                resultRepository = PackageRepositoryFactory.Default.CreateRepository(packageSource.Source);
            }
            else
            {
                // If no active repository is set, get packages from all repositories
                resultRepository = _aggregatedRepository;
            }

            return resultRepository;
        }

        private void LoadPackageSources()
        {
            _registeredPackageSources.Clear();
            var savedRepositories = _settings.PackageRepositories;
            if ((savedRepositories != null) && (savedRepositories.Length > 0))
            {
                foreach (var repositoryEntry in savedRepositories)
                {
                    var splittedEntry = repositoryEntry.Split(new[] { '=' }, 2);
                    if (splittedEntry.Length == 2)
                    {
                        if (!string.IsNullOrEmpty(splittedEntry[0]) && !string.IsNullOrEmpty(splittedEntry[1]))
                        {
                            // Create PackageSource from this entry
                            try
                            {
                                var savedPackageSource = new PackageSource(splittedEntry[1], splittedEntry[0]);
                                _registeredPackageSources.Add(savedPackageSource);
                            }
                            catch (Exception)
                            {
                                SD.Log.WarnFormatted(
                                    "[AddInManager2] URL '{0}' can't be used as valid package source.",
                                    splittedEntry[1]);
                            }
                        }
                    }
                }
            }

            AddDefaultRepository();

            // Send around the update
            _events.OnPackageSourcesChanged(new EventArgs());
        }

        private void SavePackageSources()
        {
            AddDefaultRepository();
            var savedRepositories = _registeredPackageSources.Select(ps => ps.Name + "=" + ps.Source).ToArray();
            _settings.PackageRepositories = savedRepositories;
            UpdateCurrentRepository();
        }

        private void AddDefaultRepository()
        {
            var defaultPackageSource = _registeredPackageSources
                .SingleOrDefault(packageSource => packageSource.Source == DefaultRepositorySource);

            if (defaultPackageSource == null)
            {
                var defaultRepositoryName = Resources.AddInManager2_DefaultRepository ?? DefaultRepositoryName;

                // Default repository is not configured, add it
                defaultPackageSource =
                    new PackageSource(DefaultRepositorySource, defaultRepositoryName);
                _registeredPackageSources.Insert(0, defaultPackageSource);
                SavePackageSources();
            }
        }

        private void UpdateCurrentRepository()
        {
            _registeredPackageRepositories =
                _registeredPackageSources
                    .Select(
                        packageSource => PackageRepositoryFactory.Default.CreateRepository(packageSource.Source))
                    .ToArray();

            if (_registeredPackageRepositories.Any())
            {
                _aggregatedRepository = new AggregateRepository(_registeredPackageRepositories);
            }
        }
    }
}