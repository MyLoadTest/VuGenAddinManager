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
using System.IO;
using System.Linq;
using ICSharpCode.Core;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.Model
{
    /// <summary>
    /// Wrapper around native NuGet package manager.
    /// </summary>
    public class NuGetPackageManager : INuGetPackageManager
    {
        private readonly IPackageRepositories _repositories;
        private readonly IAddInManagerEvents _events;
        private readonly ILogger _logger;
        private readonly string _packageOutputDirectory;
        private NuGetPackageManagerImplementation _packageManager;

        public NuGetPackageManager(IPackageRepositories repositories, IAddInManagerEvents events, ISDAddInManagement addInManagement)
        {
            _repositories = repositories;
            _events = events;
            _packageOutputDirectory = Path.Combine(addInManagement.ConfigDirectory, "NuGet");

            _logger = new PackageMessageLogger(_events);

            _events.PackageMessageLogged += Events_PackageMessageLogged;
        }

        public IPackageManager Packages
        {
            get
            {
                // Create PackageManager instance lazily
                return EnsurePackageManagerInstance();
            }
        }

        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public string PackageOutputDirectory
        {
            get
            {
                return _packageOutputDirectory;
            }
        }

        public IPackageOperationResolver CreateInstallPackageOperationResolver(bool allowPrereleaseVersions)
        {
            EnsurePackageManagerInstance();

            return new InstallWalker(
                _packageManager.LocalRepository, 
                _packageManager.SourceRepository, 
                null, 
                _logger, 
                false, 
                allowPrereleaseVersions, 
                DependencyVersion.Lowest);
        }

        public void ExecuteOperation(PackageOperation operation)
        {
            EnsurePackageManagerInstance();
            _packageManager.ExecuteOperation(operation);
        }

        public string GetLocalPackageDirectory(IPackage package)
        {
            return Path.Combine(PackageOutputDirectory, package.Id + "." + package.Version);
        }

        private static void Events_PackageMessageLogged(object sender, PackageMessageLoggedEventArgs e)
        {
            LoggingService.InfoFormatted("[NuGetPackageManager] {0}", e.Message);
        }

        private IPackageManager EnsurePackageManagerInstance()
        {
            if (_packageManager != null)
            {
                return _packageManager;
            }

            // Ensure that package directory exists
            if (!Directory.Exists(_packageOutputDirectory))
            {
                Directory.CreateDirectory(_packageOutputDirectory);
            }

            // Create new package manager instance
            _packageManager = new NuGetPackageManagerImplementation(_repositories.AllRegistered, _packageOutputDirectory, this);
            _packageManager.PackageInstalled += _packageEvents_NuGetPackageInstalled;
            _packageManager.PackageUninstalled += _packageEvents_NuGetPackageUninstalled;
            return _packageManager;
        }

        private void _packageEvents_NuGetPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            // NuGet package has been downloaded and extracted, now install the AddIn from it
            // TODO Error management?
            _events.OnAddInPackageDownloaded(e);
        }

        private void _packageEvents_NuGetPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            _events.OnAddInPackageRemoved(e);
        }

        private sealed class NuGetPackageManagerImplementation : PackageManager
        {
            private readonly INuGetPackageManager _internalPackageManager;

            public NuGetPackageManagerImplementation(IPackageRepository sourceRepository, string path, INuGetPackageManager internalPackageManager)
                : base(sourceRepository, path)
            {
                _internalPackageManager = internalPackageManager;
            }

            public void ExecuteOperation(PackageOperation operation)
            {
                // Allow to call this method from outside of the class
                base.Execute(operation);
            }

            public override void UninstallPackage(IPackage package, bool forceRemove, bool removeDependencies)
            {
                base.UninstallPackage(package, forceRemove, removeDependencies);
                var localPackageDirectory = _internalPackageManager.GetLocalPackageDirectory(package);
                if (Directory.Exists(localPackageDirectory))
                {
                    // Directory still exists after removing the package -> try to delete it explicitly
                    Directory.Delete(localPackageDirectory, true);
                }
            }
        }

        /// <summary>
        /// Helper for log messages generated by NuGet component.
        /// </summary>
        private sealed class PackageMessageLogger : ILogger
        {
            private readonly IAddInManagerEvents _events;

            public PackageMessageLogger(IAddInManagerEvents events)
            {
                _events = events;
            }

            public void Log(MessageLevel level, string message, params object[] args)
            {
                _events.OnPackageMessageLogged(new PackageMessageLoggedEventArgs(level, message, args));
            }

            public FileConflictResolution ResolveFileConflict(string message)
            {
                return FileConflictResolution.IgnoreAll;
            }
        }
    }
}