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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// A thread-safe service container class.
    /// </summary>
    internal sealed class SharpDevelopServiceContainer : IServiceContainer, IDisposable
    {
        private readonly ConcurrentStack<IServiceProvider> _fallbackProviders =
            new ConcurrentStack<IServiceProvider>();

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly List<Type> _servicesToDispose = new List<Type>();

        // object = TaskCompletionSource<T> for various T
        private readonly Dictionary<Type, object> _taskCompletionSources = new Dictionary<Type, object>();

        public SharpDevelopServiceContainer()
        {
            _services.Add(typeof(SharpDevelopServiceContainer), this);
            _services.Add(typeof(IServiceContainer), this);
        }

        public void AddFallbackProvider(IServiceProvider provider)
        {
            this._fallbackProviders.Push(provider);
        }

        public object GetService(Type serviceType)
        {
            object instance;
            lock (_services)
            {
                if (_services.TryGetValue(serviceType, out instance))
                {
                    var callback = instance as ServiceCreatorCallback;
                    if (callback != null)
                    {
                        SD.Log.Debug("Service startup: " + serviceType);
                        instance = callback(this, serviceType);
                        if (instance != null)
                        {
                            if (!serviceType.IsInstanceOfType(instance))
                            {
                                throw new InvalidOperationException(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        @"The service instance type '{0}' is incompatible with the specified service type '{1}'.",
                                        instance.GetType().FullName,
                                        serviceType.FullName));
                            }

                            _services[serviceType] = instance;
                            OnServiceInitialized(serviceType, instance);
                        }
                        else
                        {
                            _services.Remove(serviceType);
                        }
                    }
                }
            }

            if (instance != null)
            {
                return instance;
            }

            foreach (var fallbackProvider in _fallbackProviders)
            {
                instance = fallbackProvider.GetService(serviceType);
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }

        public void Dispose()
        {
            var loggingService = SD.Log;
            Type[] disposableTypes;
            lock (_services)
            {
                disposableTypes = _servicesToDispose.ToArray();

                // services.Clear();
                _servicesToDispose.Clear();
            }

            // dispose services in reverse order of their creation
            for (var i = disposableTypes.Length - 1; i >= 0; i--)
            {
                IDisposable disposable = null;
                lock (_services)
                {
                    object serviceInstance;
                    if (_services.TryGetValue(disposableTypes[i], out serviceInstance))
                    {
                        disposable = serviceInstance as IDisposable;
                        if (disposable != null)
                        {
                            _services.Remove(disposableTypes[i]);
                        }
                    }
                }

                if (disposable != null)
                {
                    loggingService.Debug("Service shutdown: " + disposableTypes[i]);
                    disposable.Dispose();
                }
            }
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (serviceInstance == null)
            {
                throw new ArgumentNullException("serviceInstance");
            }

            if (!serviceType.IsInstanceOfType(serviceInstance))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        @"The service instance type '{0}' is incompatible with the specified service type '{1}'.",
                        serviceInstance.GetType().FullName,
                        serviceType.FullName),
                    "serviceInstance");
            }

            lock (_services)
            {
                _services.Add(serviceType, serviceInstance);
                OnServiceInitialized(serviceType, serviceInstance);
            }
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            AddService(serviceType, serviceInstance);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            lock (_services)
            {
                _services.Add(serviceType, callback);
            }
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            AddService(serviceType, callback);
        }

        public void RemoveService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            lock (_services)
            {
                object instance;
                if (_services.TryGetValue(serviceType, out instance))
                {
                    _services.Remove(serviceType);
                    var disposableInstance = instance as IDisposable;
                    if (disposableInstance != null)
                    {
                        _servicesToDispose.Remove(serviceType);
                    }
                }
            }
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            RemoveService(serviceType);
        }

        public Task<T> GetFutureService<T>()
        {
            var serviceType = typeof(T);
            lock (_services)
            {
                if (_services.ContainsKey(serviceType))
                {
                    return Task.FromResult((T)GetService(serviceType));
                }

                object taskCompletionSource;
                if (_taskCompletionSources.TryGetValue(serviceType, out taskCompletionSource))
                {
                    return ((TaskCompletionSource<T>)taskCompletionSource).Task;
                }

                var tcs = new TaskCompletionSource<T>();
                _taskCompletionSources.Add(serviceType, tcs);
                return tcs.Task;
            }
        }

        private void OnServiceInitialized(Type serviceType, object serviceInstance)
        {
            var disposableService = serviceInstance as IDisposable;
            if (disposableService != null)
            {
                _servicesToDispose.Add(serviceType);
            }

            dynamic taskCompletionSource;
            if (_taskCompletionSources.TryGetValue(serviceType, out taskCompletionSource))
            {
                _taskCompletionSources.Remove(serviceType);
                taskCompletionSource.SetResult((dynamic)serviceInstance);
            }
        }
    }
}