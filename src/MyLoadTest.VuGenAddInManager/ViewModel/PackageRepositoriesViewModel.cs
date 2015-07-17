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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using MyLoadTest.VuGenAddInManager.Compatibility;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using NuGet;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    public sealed class PackageRepositoriesViewModel : Model<PackageRepositoriesViewModel>
    {
        private readonly ObservableCollection<PackageRepository> _packageRepositories =
            new ObservableCollection<PackageRepository>();

        private readonly PackageRepository _newPackageSource = new PackageRepository();

        private ObservableCollection<PackageSource> _packageSources;

        private DelegateCommand _addPackageSourceCommmand;
        private DelegateCommand _removePackageSourceCommand;
        private DelegateCommand _movePackageSourceUpCommand;
        private DelegateCommand _movePackageSourceDownCommand;
        private DelegateCommand _browsePackageFolderCommand;

        private PackageRepository _selectedPackageRepository;

        public PackageRepositoriesViewModel()
        {
            Initialize();
        }

        public PackageRepositoriesViewModel(IAddInManagerServices services)
            : base(services)
        {
            Initialize();
        }

        public ICommand AddPackageSourceCommand
        {
            get
            {
                return _addPackageSourceCommmand;
            }
        }

        public ICommand RemovePackageSourceCommand
        {
            get
            {
                return _removePackageSourceCommand;
            }
        }

        public ICommand MovePackageSourceUpCommand
        {
            get
            {
                return _movePackageSourceUpCommand;
            }
        }

        public ICommand MovePackageSourceDownCommand
        {
            get
            {
                return _movePackageSourceDownCommand;
            }
        }

        public ICommand BrowsePackageFolderCommand
        {
            get
            {
                return _browsePackageFolderCommand;
            }
        }

        public ObservableCollection<PackageRepository> PackageRepositories
        {
            get
            {
                return _packageRepositories;
            }
        }

        public string NewPackageSourceName
        {
            get
            {
                return _newPackageSource.Name;
            }

            set
            {
                _newPackageSource.Name = value;
                OnPropertyChanged(viewModel => viewModel.NewPackageSourceName);
            }
        }

        public string NewPackageSourceUrl
        {
            get
            {
                return _newPackageSource.SourceUrl;
            }

            set
            {
                _newPackageSource.SourceUrl = value;
                OnPropertyChanged(viewModel => viewModel.NewPackageSourceUrl);
            }
        }

        public PackageRepository SelectedPackageRepository
        {
            get
            {
                return _selectedPackageRepository;
            }

            set
            {
                _selectedPackageRepository = value;
                OnPropertyChanged(viewModel => viewModel.SelectedPackageRepository);
                OnPropertyChanged(viewModel => viewModel.CanAddPackageSource);
            }
        }

        public bool CanAddPackageSource
        {
            get
            {
                return NewPackageSourceHasUrl && NewPackageSourceHasName;
            }
        }

        public bool CanRemovePackageSource
        {
            get
            {
                return _selectedPackageRepository != null;
            }
        }

        public bool CanMovePackageSourceUp
        {
            get
            {
                return HasAtLeastTwoPackageSources() && !IsFirstPackageSourceSelected();
            }
        }

        public bool CanMovePackageSourceDown
        {
            get
            {
                return HasAtLeastTwoPackageSources() && !IsLastPackageSourceSelected();
            }
        }

        private bool NewPackageSourceHasUrl
        {
            get
            {
                return !string.IsNullOrEmpty(NewPackageSourceUrl);
            }
        }

        private bool NewPackageSourceHasName
        {
            get
            {
                return !string.IsNullOrEmpty(NewPackageSourceName);
            }
        }

        public void Load()
        {
            _packageSources.Clear();
            CollectionExtensions.AddRange(_packageSources, AddInManager.Repositories.RegisteredPackageSources);
            foreach (var packageSource in _packageSources)
            {
                AddPackageSourceToViewModel(packageSource);
            }
        }

        public void Save()
        {
            _packageSources.Clear();
            foreach (var packageRepository in _packageRepositories)
            {
                var source = packageRepository.ToPackageSource();
                _packageSources.Add(source);
            }

            AddInManager.Repositories.RegisteredPackageSources = _packageSources;
        }

        public void AddPackageSource()
        {
            AddNewPackageSourceToViewModel();
            SelectLastPackageSourceViewModel();
        }

        public void RemovePackageSource()
        {
            RemoveSelectedPackageSourceViewModel();
        }

        public void MovePackageSourceUp()
        {
            var selectedPackageSourceIndex = GetSelectedPackageSourceViewModelIndex();
            var destinationPackageSourceIndex = selectedPackageSourceIndex--;
            _packageRepositories.Move(selectedPackageSourceIndex, destinationPackageSourceIndex);
        }

        public void MovePackageSourceDown()
        {
            var selectedPackageSourceIndex = GetSelectedPackageSourceViewModelIndex();
            var destinationPackageSourceIndex = selectedPackageSourceIndex++;
            _packageRepositories.Move(selectedPackageSourceIndex, destinationPackageSourceIndex);
        }

        public void BrowsePackageFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var owner = SD.WinForms.MainWin32Window;
                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    UpdateNewPackageSourceUsingSelectedFolder(dialog.SelectedPath);
                }
            }
        }

        private static string GetPackageSourceNameFromFolder(string folder)
        {
            return Path.GetFileName(folder);
        }

        private void AddPackageSourceToViewModel(PackageSource packageSource)
        {
            var packageRepository = new PackageRepository(packageSource);
            _packageRepositories.Add(packageRepository);
        }

        private void AddNewPackageSourceToViewModel()
        {
            var packageSource = _newPackageSource.ToPackageSource();
            AddPackageSourceToViewModel(packageSource);
        }

        private void SelectLastPackageSourceViewModel()
        {
            SelectedPackageRepository = GetLastPackageSourceViewModel();
        }

        private void RemoveSelectedPackageSourceViewModel()
        {
            _packageRepositories.Remove(_selectedPackageRepository);
        }

        private int GetSelectedPackageSourceViewModelIndex()
        {
            return _packageRepositories.IndexOf(_selectedPackageRepository);
        }

        private bool IsFirstPackageSourceSelected()
        {
            return _selectedPackageRepository == _packageRepositories[0];
        }

        private bool HasAtLeastTwoPackageSources()
        {
            return _packageRepositories.Count >= 2;
        }

        private bool IsLastPackageSourceSelected()
        {
            var lastViewModel = GetLastPackageSourceViewModel();
            return lastViewModel == _selectedPackageRepository;
        }

        private void Initialize()
        {
            this._packageSources = new ObservableCollection<PackageSource>();
            CreateCommands();
        }

        private void CreateCommands()
        {
            _addPackageSourceCommmand =
                new DelegateCommand(
                    param => AddPackageSource(),
                    param => CanAddPackageSource);

            _removePackageSourceCommand =
                new DelegateCommand(
                    param => RemovePackageSource(),
                    param => CanRemovePackageSource);

            _movePackageSourceUpCommand =
                new DelegateCommand(
                    param => MovePackageSourceUp(),
                    param => CanMovePackageSourceUp);

            _movePackageSourceDownCommand =
                new DelegateCommand(
                    param => MovePackageSourceDown(),
                    param => CanMovePackageSourceDown);

            _browsePackageFolderCommand =
                new DelegateCommand(param => BrowsePackageFolder());
        }

        private PackageRepository GetLastPackageSourceViewModel()
        {
            return _packageRepositories.Last();
        }

        private void UpdateNewPackageSourceUsingSelectedFolder(string folder)
        {
            NewPackageSourceUrl = folder;
            NewPackageSourceName = GetPackageSourceNameFromFolder(folder);
        }
    }
}