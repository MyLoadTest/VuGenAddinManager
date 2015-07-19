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
using ICSharpCode.Core;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    public sealed class PropertyServiceImpl : IPropertyService
    {
        private readonly ICSharpCode.Core.Properties _properties;

        public PropertyServiceImpl(ICSharpCode.Core.Properties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            this._properties = properties;
        }

        public DirectoryName ConfigDirectory
        {
            get
            {
                return new DirectoryName(PropertyService.ConfigDirectory);
            }
        }

        public DirectoryName DataDirectory
        {
            get
            {
                return new DirectoryName(PropertyService.DataDirectory);
            }
        }

        public T Get<T>(string key, T defaultValue)
        {
            return _properties.Get(key, defaultValue);
        }

        [Obsolete("Use the NestedProperties method instead", true)]
        public ICSharpCode.Core.Properties Get(string key, ICSharpCode.Core.Properties defaultValue)
        {
            return _properties.Get(key, defaultValue);
        }

        public ICSharpCode.Core.Properties NestedProperties(string key)
        {
            throw new NotImplementedException();
            ////return properties.NestedProperties(key);
        }

        public void SetNestedProperties(string key, ICSharpCode.Core.Properties nestedProperties)
        {
            throw new NotImplementedException();
            ////properties.SetNestedProperties(key, nestedProperties);
        }

        public bool Contains(string key)
        {
            return _properties.Contains(key);
        }

        public void Set<T>(string key, T value)
        {
            _properties.Set(key, value);
        }

        public IReadOnlyList<T> GetList<T>(string key)
        {
            throw new NotImplementedException();
            ////return _properties.GetList<T>(key);
        }

        public void SetList<T>(string key, IEnumerable<T> value)
        {
            throw new NotImplementedException();
            ////properties.SetList(key, value);
        }

        public void Remove(string key)
        {
            _properties.Remove(key);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _properties.PropertyChanged +=
                    (sender, args) =>
                        value(sender, new System.ComponentModel.PropertyChangedEventArgs(args.Key));
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public ICSharpCode.Core.Properties MainPropertiesContainer
        {
            get
            {
                return _properties;
            }
        }

        public void Save()
        {
        }

        public ICSharpCode.Core.Properties LoadExtraProperties(string key)
        {
            return new ICSharpCode.Core.Properties();
        }

        public void SaveExtraProperties(string key, ICSharpCode.Core.Properties p)
        {
        }
    }
}