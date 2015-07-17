using System;
using System.Collections.Generic;
using System.Linq;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    internal sealed class FallbackServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _fallbackServiceDict = new Dictionary<Type, object>();

        public object GetService(Type serviceType)
        {
            object instance;
            lock (_fallbackServiceDict)
            {
                if (!_fallbackServiceDict.TryGetValue(serviceType, out instance))
                {
                    var attrs = serviceType.GetCustomAttributes(typeof(SDServiceAttribute), false);
                    if (attrs.Length == 1)
                    {
                        var attr = (SDServiceAttribute)attrs[0];
                        if (attr.FallbackImplementation != null)
                        {
                            instance = Activator.CreateInstance(attr.FallbackImplementation);
                        }
                    }

                    // store null if no fallback implementation exists
                    _fallbackServiceDict.Add(serviceType, instance);
                }
            }

            return instance;
        }
    }
}