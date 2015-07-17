using System;
using System.Linq;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// Specifies that the interface is a SharpDevelop service that is accessible via <c>SD.Services</c>.
    /// </summary>
    /// <remarks>
    /// This attribute is mostly intended as documentation, so that it is easily possible to see
    /// if a given service is globally available in SharpDevelop.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false)]
    //// ReSharper disable once InconsistentNaming - SD 5.0 Compatibility
    public class SDServiceAttribute : Attribute
    {
        public SDServiceAttribute()
        {
        }

        public SDServiceAttribute(string staticPropertyPath)
        {
            this.StaticPropertyPath = staticPropertyPath;
        }

        public string StaticPropertyPath
        {
            get;
            set;
        }

        public Type FallbackImplementation
        {
            get;
            set;
        }
    }
}