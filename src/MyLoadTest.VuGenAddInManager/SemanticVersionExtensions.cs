using System;
using System.Linq;
using NuGet;

namespace MyLoadTest.VuGenAddInManager
{
    public static class SemanticVersionExtensions
    {
        public static Version ToVersion(this SemanticVersion semanticVersion)
        {
            var versionString = semanticVersion.ToString();
            if (!string.IsNullOrEmpty(semanticVersion.SpecialVersion))
            {
                // Remove special version from string (-1 for the "-" added before the version)
                versionString = versionString.Substring(0, versionString.Length - semanticVersion.SpecialVersion.Length - 1);
            }

            return new Version(versionString);
        }
    }
}