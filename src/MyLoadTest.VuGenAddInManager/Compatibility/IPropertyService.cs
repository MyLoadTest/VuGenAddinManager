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
using System.ComponentModel;
using System.Linq;
using ICSharpCode.Core;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// The property service.
    /// </summary>
    [SDService("SD.PropertyService")]
    public interface IPropertyService : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the configuration directory. (usually "%ApplicationData%\%ApplicationName%")
        /// </summary>
        DirectoryName ConfigDirectory { get; }

        /// <summary>
        /// Gets the data directory (usually "ApplicationRootPath\data")
        /// </summary>
        DirectoryName DataDirectory { get; }

        /// <summary>
        /// Gets the main properties container for this property service.
        /// </summary>
        ICSharpCode.Core.Properties MainPropertiesContainer { get; }

        /// <inheritdoc cref="Properties.Get{T}(string, T)"/>
        T Get<T>(string key, T defaultValue);

        ICSharpCode.Core.Properties NestedProperties(string key);

        void SetNestedProperties(string key, ICSharpCode.Core.Properties nestedProperties);

        /// <inheritdoc cref="Properties.Contains"/>
        bool Contains(string key);

        /// <inheritdoc cref="Properties.Set{T}(string, T)"/>
        void Set<T>(string key, T value);

        IReadOnlyList<T> GetList<T>(string key);

        void SetList<T>(string key, IEnumerable<T> value);

        /// <inheritdoc cref="Properties.Remove"/>
        void Remove(string key);

        /// <summary>
        /// Saves the main properties to disk.
        /// </summary>
        void Save();

        /// <summary>
        /// Loads extra properties that are not part of the main properties container.
        /// Unlike <see cref="NestedProperties"/>, multiple calls to <see cref="LoadExtraProperties"/>
        /// will return different instances, as the properties are re-loaded from disk every time.
        /// To save the properties, you need to call <see cref="SaveExtraProperties"/>.
        /// </summary>
        /// <returns>Properties container that was loaded; or an empty properties container
        /// if no container with the specified key exists.</returns>
        ICSharpCode.Core.Properties LoadExtraProperties(string key);

        /// <summary>
        /// Saves extra properties that are not part of the main properties container.
        /// </summary>
        void SaveExtraProperties(string key, ICSharpCode.Core.Properties p);
    }
}