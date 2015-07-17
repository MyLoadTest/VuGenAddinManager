// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using System.Linq;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;

namespace MyLoadTest.VuGenAddInManager
{
    /// <summary>
    /// Container for public services of AddInManager AddIn.
    /// </summary>
    public sealed class AddInManagerServices
    {
        private static readonly AddInManagerServiceContainer Container;

        static AddInManagerServices()
        {
            Container = new AddInManagerServiceContainer
            {
                Settings = new AddInManagerSettings(),
                Events = new AddInManagerEvents(),
                SDAddInManagement = new SDAddInManagement()
            };

            Container.Repositories = new PackageRepositories(Container.Events, Container.Settings);
            Container.NuGet = new NuGetPackageManager(Container.Repositories, Container.Events, Container.SDAddInManagement);
            Container.Setup = new AddInSetup(Container.Events, Container.NuGet, Container.SDAddInManagement);
        }

        public static IAddInManagerEvents Events
        {
            get
            {
                return Container.Events;
            }
        }

        public static IPackageRepositories Repositories
        {
            get
            {
                return Container.Repositories;
            }
        }

        public static IAddInSetup Setup
        {
            get
            {
                return Container.Setup;
            }
        }

        public static INuGetPackageManager NuGet
        {
            get
            {
                return Container.NuGet;
            }
        }

        public static IAddInManagerSettings Settings
        {
            get
            {
                return Container.Settings;
            }
        }

        public static IAddInManagerServices Services
        {
            get
            {
                return Container;
            }
        }

        private sealed class AddInManagerServiceContainer : IAddInManagerServices
        {
            public IAddInManagerEvents Events
            {
                get;
                set;
            }

            public IPackageRepositories Repositories
            {
                get;
                set;
            }

            public IAddInSetup Setup
            {
                get;
                set;
            }

            public INuGetPackageManager NuGet
            {
                get;
                set;
            }

            public IAddInManagerSettings Settings
            {
                get;
                set;
            }

            public ISDAddInManagement SDAddInManagement
            {
                get;
                set;
            }
        }
    }
}