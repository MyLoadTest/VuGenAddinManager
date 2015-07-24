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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ICSharpCode.Core;
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Properties;
using MyLoadTest.VuGenAddInManager.View;
using NuGet;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    public sealed class AddInManagerViewModel : Model<AddInManagerViewModel>, IDisposable
    {
        private string _message;
        private bool _hasError;

        private ObservableCollection<AddInsViewModelBase> _viewModels;

        public AddInManagerViewModel()
        {
            Initialize();
        }

        public AvailableAddInsViewModel AvailableAddInsViewModel
        {
            get;
            private set;
        }

        public InstalledAddInsViewModel InstalledAddInsViewModel
        {
            get;
            private set;
        }

        public UpdatedAddInsViewModel UpdatedAddInsViewModel
        {
            get;
            private set;
        }

        public ObservableCollection<AddInsViewModelBase> ViewModels
        {
            get
            {
                return _viewModels;
            }
        }

        public string Title
        {
            get;
            private set;
        }

        public void Dispose()
        {
            AddInManager.Events.OperationStarted -= AddInManager_Events_OperationStarted;
            AddInManager.Events.AddInOperationError -= AddInManager_Events_AddInOperationError;
            AddInManager.Events.AcceptLicenses -= AddInManager_Events_AcceptLicenses;
            foreach (var viewModel in _viewModels)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
                OnPropertyChanged(model => model.Message);
            }
        }

        public bool HasError
        {
            get
            {
                return _hasError;
            }

            set
            {
                _hasError = value;
                OnPropertyChanged(model => model.HasError);
            }
        }

        private static void ShowErrorMessage(string message)
        {
            SD.MessageService.ShowWarning(StringParser.Parse(message));
        }

        private static void AddInManager_Events_AddInOperationError(object sender, AddInOperationErrorEventArgs e)
        {
            ShowErrorMessage(e.Message);
        }

        private static void AddInManager_Events_AcceptLicenses(object sender, AcceptLicensesEventArgs e)
        {
            // Show a license acceptance prompt to the user
            e.IsAccepted = ShowLicenseAcceptancePrompt(e.Packages);
        }

        private static bool ShowLicenseAcceptancePrompt(IEnumerable<IPackage> packages)
        {
            if (packages == null)
            {
                // No package -> nothing to accept
                return true;
            }

            // Create a license acceptance view
            var viewModel = new LicenseAcceptanceViewModel(packages);
            var view = new LicenseAcceptanceView
            {
                DataContext = viewModel,
                Owner = SD.Workbench.MainWindow
            };

            return view.ShowDialog() ?? false;
        }

        private void Initialize()
        {
            if (WpfHelper.IsInDesignMode())
            {
                return;
            }

            Title = Resources.AddInManager_Title;

            AddInManager.Events.OperationStarted += AddInManager_Events_OperationStarted;
            AddInManager.Events.AddInOperationError += AddInManager_Events_AddInOperationError;
            AddInManager.Events.AcceptLicenses += AddInManager_Events_AcceptLicenses;

            InstalledAddInsViewModel = new InstalledAddInsViewModel();
            AvailableAddInsViewModel = new AvailableAddInsViewModel();
            UpdatedAddInsViewModel = new UpdatedAddInsViewModel();

            _viewModels = new ObservableCollection<AddInsViewModelBase>
            {
                InstalledAddInsViewModel,
                AvailableAddInsViewModel,
                UpdatedAddInsViewModel
            };

            foreach (var viewModel in _viewModels)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            AddInManager.Events.OnAddInManagerViewOpened();

            // Expand the first view
            InstalledAddInsViewModel.IsExpandedInView = true;

            // Read the packages
            AvailableAddInsViewModel.ReadPackages();
            InstalledAddInsViewModel.ReadPackages();
            UpdatedAddInsViewModel.ReadPackages();
        }

        private void AddInManager_Events_OperationStarted(object sender, EventArgs e)
        {
            ClearMessage();
        }

        private void ClearMessage()
        {
            Message = null;
            HasError = false;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExpandedInView")
            {
                var expandedViewModel = sender as AddInsViewModelBase;
                if (expandedViewModel != null)
                {
                    if (expandedViewModel.IsExpandedInView)
                    {
                        // Unexpand all view models besides this one
                        foreach (var viewModel in _viewModels)
                        {
                            if (viewModel != expandedViewModel)
                            {
                                viewModel.IsExpandedInView = false;
                            }
                        }
                    }
                    else if (!expandedViewModel.IsExpandedInView && (_viewModels.Count(v => v.IsExpandedInView) == 0))
                    {
                        // This is the last unexpanded view => leave it open
                        expandedViewModel.IsExpandedInView = true;
                    }
                }
            }
        }
    }
}