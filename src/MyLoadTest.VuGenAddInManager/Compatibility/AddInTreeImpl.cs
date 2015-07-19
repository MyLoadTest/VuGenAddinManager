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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ICSharpCode.Core;

namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// Class containing the AddInTree. Contains methods for accessing tree nodes and building items.
    /// </summary>
    public sealed class AddInTreeImpl : IAddInTree
    {
        /// <summary>
        /// Gets the list of loaded AddIns.
        /// </summary>
        public IReadOnlyList<AddIn> AddIns
        {
            get
            {
                var addIns = AddInTree.AddIns;
                return addIns as IReadOnlyList<AddIn> ?? new ReadOnlyCollection<AddIn>(addIns);
            }
        }

        /// <summary>
        /// Gets a dictionary of registered doozers.
        /// </summary>
        public ConcurrentDictionary<string, IDoozer> Doozers
        {
            get
            {
                return AddInTree.Doozers;
            }
        }

        /// <summary>
        /// Gets a dictionary of registered condition evaluators.
        /// </summary>
        public ConcurrentDictionary<string, IConditionEvaluator> ConditionEvaluators
        {
            get
            {
                return AddInTree.ConditionEvaluators;
            }
        }

        /// <summary>
        /// Gets the <see cref="AddInTreeNode"/> representing the specified path.
        /// </summary>
        /// <param name="path">The path of the AddIn tree node</param>
        /// <param name="throwOnNotFound">
        /// If set to <c>true</c>, this method throws a
        /// <see cref="TreePathNotFoundException"/> when the path does not exist.
        /// If set to <c>false</c>, <c>null</c> is returned for non-existing paths.
        /// </param>
        public AddInTreeNode GetTreeNode(string path, bool throwOnNotFound = true)
        {
            return AddInTree.GetTreeNode(path, throwOnNotFound);
        }

        /// <summary>
        /// Builds a single item in the addin tree.
        /// </summary>
        /// <param name="path">A path to the item in the addin tree.</param>
        /// <param name="parameter">A parameter that gets passed into the doozer and condition evaluators.</param>
        /// <exception cref="TreePathNotFoundException">The path does not
        /// exist or does not point to an item.</exception>
        public object BuildItem(string path, object parameter)
        {
            return AddInTree.BuildItem(path, parameter);
        }

        public object BuildItem(string path, object parameter, IEnumerable<ICondition> additionalConditions)
        {
            return AddInTree.BuildItem(path, parameter, additionalConditions);
        }

        /// <summary>
        /// Builds the items in the path. Ensures that all items have the type T.
        /// </summary>
        /// <param name="path">A path in the addin tree.</param>
        /// <param name="parameter">The owner used to create the objects.</param>
        /// <param name="throwOnNotFound">If true, throws a <see cref="TreePathNotFoundException"/>
        /// if the path is not found. If false, an empty ArrayList is returned when the
        /// path is not found.</param>
        public IReadOnlyList<T> BuildItems<T>(string path, object parameter, bool throwOnNotFound = true)
        {
            return AddInTree.BuildItems<T>(path, parameter, throwOnNotFound).AsReadOnly();
        }

        /// <summary>
        /// The specified AddIn is added to the <see cref="AddIns"/> collection.
        /// If the AddIn is enabled, its doozers, condition evaluators and extension
        /// paths are added to the AddInTree and its resources are added to the
        /// <see cref="ResourceService"/>.
        /// </summary>
        public void InsertAddIn(AddIn addIn)
        {
            AddInTree.InsertAddIn(addIn);
        }

        /// <summary>
        /// The specified AddIn is removed to the <see cref="AddIns"/> collection.
        /// This is only possible for disabled AddIns, enabled AddIns require
        /// a restart of the application to be removed.
        /// </summary>
        /// <exception cref="ArgumentException">Occurs when trying to remove an enabled AddIn.</exception>
        public void RemoveAddIn(AddIn addIn)
        {
            AddInTree.RemoveAddIn(addIn);
        }

        /// <summary>
        /// Loads a list of .addin files, ensuring that dependencies are satisfied.
        /// This method is normally called by <see cref="CoreStartup.RunInitialization"/>.
        /// </summary>
        /// <param name="addInFiles">
        /// The list of .addin file names to load.
        /// </param>
        /// <param name="disabledAddIns">
        /// The list of disabled AddIn identity names.
        /// </param>
        public void Load(List<string> addInFiles, List<string> disabledAddIns)
        {
            AddInTree.Load(addInFiles, disabledAddIns);
        }
    }
}