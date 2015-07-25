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
using ICSharpCode.Core;
using MyLoadTest.VuGenAddInManager.Model;
using MyLoadTest.VuGenAddInManager.Model.Interfaces;
using MyLoadTest.VuGenAddInManager.Properties;

namespace MyLoadTest.VuGenAddInManager.ViewModel
{
    /// <summary>
    /// View model representing an already installed real SharpDevelop AddIn.
    /// </summary>
    public sealed class OfflineAddInsViewModel : AddInPackageViewModelBase
    {
        private AddIn _addIn;
        private ManagedAddIn _markedAddIn;

        private string _name;
        private Uri _licenseUrl;
        private Uri _projectUrl;
        private Uri _reportAbuseUrl;
        private IEnumerable<AddInDependency> _dependencies;
        private IEnumerable<string> _authors;
        private bool _hasDownloadCount;
        private string _id;
        private Uri _iconUrl;
        private string _summary;
        private Version _version;
        private Version _oldVersion;
        private int _downloadCount;
        private string _description;
        private DateTime? _lastUpdated;

        public OfflineAddInsViewModel(ManagedAddIn addIn)
        {
            Initialize(addIn);
        }

        public OfflineAddInsViewModel(IAddInManagerServices services, ManagedAddIn addIn)
            : base(services)
        {
            Initialize(addIn);
        }

        public AddIn AddIn
        {
            get
            {
                return _addIn;
            }
        }

        public override string Name
        {
            get
            {
                return _name;
            }
        }

        public override Uri LicenseUrl
        {
            get
            {
                return _licenseUrl;
            }
        }

        public override Uri ProjectUrl
        {
            get
            {
                return _projectUrl;
            }
        }

        public override Uri ReportAbuseUrl
        {
            get
            {
                return _reportAbuseUrl;
            }
        }

        public override bool IsOffline
        {
            get
            {
                return true;
            }
        }

        public override bool IsExternallyReferenced
        {
            get
            {
                // The AddIn is externally referenced, if it's .addin file doesn't reside in
                // somewhere in application path (preinstalled AddIns) or in config path (usually installed AddIns).
                return _addIn != null && AddInManager.SDAddInManagement.IsAddInManifestInExternalPath(_addIn);
            }
        }

        public override bool IsPreinstalled
        {
            get
            {
                return _addIn != null && AddInManager.Setup.IsAddInPreinstalled(_addIn);
            }
        }

        public override bool IsAdded
        {
            get
            {
                // return (_addIn.Action == AddInAction.Install) || (_addIn.Action == AddInAction.Update);
                return _addIn != null && _markedAddIn.IsTemporary;
            }
        }

        public override bool IsUpdate
        {
            get
            {
                return _markedAddIn.IsUpdate;
            }
        }

        public override bool IsInstalled
        {
            get
            {
                return _addIn != null && AddInManager.Setup.IsAddInInstalled(_addIn);
            }
        }

        public override bool IsInstallable
        {
            get
            {
                return false;
            }
        }

        public override bool IsUninstallable
        {
            get
            {
                return !IsPreinstalled && (Id != null);
            }
        }

        public override bool IsDisablingPossible
        {
            get
            {
                // Disabling is only possible if this AddIn has an identity and is not the AddInManager itself
                return (Id != null) && (_addIn.Manifest.PrimaryIdentity != "ICSharpCode.AddInManager2");
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return _addIn != null && _addIn.Action != AddInAction.Disable;
            }
        }

        public override bool IsRemoved
        {
            get
            {
                return _addIn != null && (!_markedAddIn.IsTemporary && (_addIn.Action == AddInAction.Uninstall));
            }
        }

        public override IEnumerable<AddInDependency> Dependencies
        {
            get
            {
                return _dependencies;
            }
        }

        public override IEnumerable<string> Authors
        {
            get
            {
                return _authors;
            }
        }

        public override bool HasDownloadCount
        {
            get
            {
                return _hasDownloadCount;
            }
        }

        public override string Id
        {
            get
            {
                return _id;
            }
        }

        public override Uri IconUrl
        {
            get
            {
                return _iconUrl;
            }
        }

        public override string Summary
        {
            get
            {
                if (_addIn == null)
                {
                    return null;
                }

                if (_addIn.Action == AddInAction.Install)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInInstalled);
                }

                if (_addIn.Action == AddInAction.Update)
                {
                    return Resources.AddInManager_AddInUpdated;
                }

                if (HasDependencyConflicts)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInDependencyFailed);
                }

                if (IsRemoved)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInRemoved);
                }

                if (IsEnabled && !_addIn.Enabled)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInEnabled);
                }

                if (!IsEnabled)
                {
                    return SurroundWithParantheses(
                        _addIn.Enabled
                            ? Resources.AddInManager_AddInWillBeDisabled
                            : Resources.AddInManager_AddInDisabled);
                }

                if (_addIn.Action == AddInAction.InstalledTwice)
                {
                    return SurroundWithParantheses(Resources.AddInManager_AddInInstalledTwice);
                }

                return _summary;
            }
        }

        public override Version Version
        {
            get
            {
                return _version;
            }
        }

        public override Version OldVersion
        {
            get
            {
                return _oldVersion;
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
                return _downloadCount;
            }
        }

        public override string Description
        {
            get
            {
                return _description;
            }
        }

        public override DateTime? LastUpdated
        {
            get
            {
                return _lastUpdated;
            }
        }

        public override string FileName
        {
            get
            {
                return _addIn != null ? FileUtility.NormalizePath(_addIn.FileName) : null;
            }
        }

        public override bool HasDependencyConflicts
        {
            get
            {
                return _addIn != null && _addIn.Action == AddInAction.DependencyError;
            }
        }

        public override bool HasNuGetConnection
        {
            get
            {
                return _markedAddIn != null
                    && _markedAddIn.InstallationSource == AddInInstallationSource.NuGetRepository;
            }
        }

        public void UpdateMembers()
        {
            if ((_addIn == null) || (_markedAddIn == null))
            {
                return;
            }

            _id = _addIn.Manifest.PrimaryIdentity;
            _name = _addIn.Name;
            if (HasNuGetConnection && (_markedAddIn.LinkedNuGetPackageVersion != null))
            {
                _version = new Version(_markedAddIn.LinkedNuGetPackageVersion);
            }
            else
            {
                if (_addIn.Version != null)
                {
                    _version = _addIn.Version;
                }
            }

            _description = _addIn.Properties["description"];
            _summary = _addIn.Properties["description"];

            var projectUrlString = _addIn.Properties["url"];
            if (!string.IsNullOrEmpty(projectUrlString))
            {
                _projectUrl = new Uri(projectUrlString, UriKind.RelativeOrAbsolute);
            }

            var licenseUrlString = _addIn.Properties["license"];
            if (!string.IsNullOrEmpty(licenseUrlString))
            {
                _licenseUrl = new Uri(licenseUrlString);
            }

            var author = _addIn.Properties["author"];
            if (!string.IsNullOrEmpty(author))
            {
                _authors = new[] { author };
            }

            if ((_addIn.Manifest != null) && (_addIn.Manifest.Dependencies != null))
            {
                _dependencies = _addIn.Manifest.Dependencies.Select(d => new AddInDependency(d));
            }

            if (_markedAddIn.IsUpdate)
            {
                _oldVersion = _markedAddIn.OldVersion;
            }

            _iconUrl = null;
            _hasDownloadCount = false;
            _downloadCount = 0;
            _lastUpdated = null;
            _reportAbuseUrl = null;
        }

        public override void AddPackage()
        {
        }

        public override void RemovePackage()
        {
            ClearReportedMessages();

            if (_addIn.Manifest.PrimaryIdentity == "ICSharpCode.AddInManager2")
            {
                MessageService.ShowMessage(
                    "${res:AddInManager2.CannotRemoveAddInManager}",
                    "${res:AddInManager.Title}");
                return;
            }

            if (!IsRemoved)
            {
                var dependentAddIns = AddInManager.Setup.GetDependentAddIns(_addIn);
                if (dependentAddIns.Any())
                {
                    var addInNames = string.Empty;
                    foreach (var dependentAddIn in dependentAddIns)
                    {
                        addInNames += "\t " + dependentAddIn.AddIn.Name + Environment.NewLine;
                    }

                    if (!MessageService.AskQuestionFormatted(
                        "${res:AddInManager.Title}",
                        "${res:AddInManager2.DisableDependentWarning}",
                        _addIn.Name,
                        addInNames))
                    {
                        return;
                    }
                }
            }

            AddInManager.Setup.UninstallAddIn(_addIn);
        }

        public override void CancelInstallation()
        {
            ClearReportedMessages();

            AddInManager.Setup.CancelInstallation(_addIn);
        }

        public override void CancelUpdate()
        {
            ClearReportedMessages();

            AddInManager.Setup.CancelUpdate(_addIn);
        }

        public override void CancelUninstallation()
        {
            ClearReportedMessages();

            AddInManager.Setup.CancelUninstallation(_addIn);
        }

        public override void DisablePackage()
        {
            ClearReportedMessages();

            if (_addIn == null)
            {
                return;
            }

            if (_addIn.Manifest.PrimaryIdentity == "ICSharpCode.AddInManager2")
            {
                MessageService.ShowMessage(
                    "${res:AddInManager.CannotDisableAddInManager}",
                    "${res:AddInManager.Title}");
                return;
            }

            if (IsEnabled)
            {
                var dependentAddIns = AddInManager.Setup.GetDependentAddIns(_addIn);
                if ((dependentAddIns != null) && dependentAddIns.Any())
                {
                    var addInNames = string.Empty;
                    foreach (var dependentAddIn in dependentAddIns)
                    {
                        addInNames += "\t " + dependentAddIn.AddIn.Name + Environment.NewLine;
                    }

                    if (!MessageService.AskQuestionFormatted(
                        "${res:AddInManager.Title}",
                        "${res:AddInManager2.DisableDependentWarning}",
                        _addIn.Name,
                        addInNames))
                    {
                        return;
                    }
                }
            }

            AddInManager.Setup.SwitchAddInActivation(_addIn);
        }

        public override bool HasOptions
        {
            get
            {
                if (_addIn.Enabled)
                {
                    foreach (var pair in _addIn.Paths)
                    {
                        if (pair.Key.StartsWith("/SharpDevelop/Dialogs/OptionsDialog"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public override void ShowOptions()
        {
            ClearReportedMessages();

            var dummyNode = new AddInTreeNode();
            foreach (var pair in _addIn.Paths)
            {
                if (pair.Key.StartsWith("/SharpDevelop/Dialogs/OptionsDialog"))
                {
                    dummyNode.AddCodons(pair.Value.Codons);
                }
            }

            ICSharpCode.SharpDevelop.Commands.OptionsCommand.ShowTabbedOptions(
                _addIn.Name + " " + Resources.AddInManager_Options,
                dummyNode);
        }

        private void Initialize(ManagedAddIn addIn)
        {
            _markedAddIn = addIn;
            if (_markedAddIn != null)
            {
                _addIn = addIn.AddIn;
            }

            if (_addIn != null)
            {
                UpdateMembers();
            }
        }

        private void ClearReportedMessages()
        {
            // Notify about new operation
            AddInManager.Events.OnOperationStarted();
        }
    }
}