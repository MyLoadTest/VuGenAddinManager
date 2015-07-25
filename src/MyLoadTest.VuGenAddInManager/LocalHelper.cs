using System;
using System.Linq;
using ICSharpCode.Core;

namespace MyLoadTest.VuGenAddInManager
{
    internal static class LocalHelper
    {
        #region Public Methods

        public static bool IsPreinstalled(AddIn addIn)
        {
            if (addIn == null)
            {
                return false;
            }

            var isMarkedPreinstalled = string.Equals(
                addIn.Properties[LocalConstants.AddInProperties.AddInManagerHidden],
                "preinstalled",
                StringComparison.OrdinalIgnoreCase);

            return isMarkedPreinstalled
                && FileUtility.IsBaseDirectory(FileUtility.ApplicationRootPath, addIn.FileName);
        }

        #endregion
    }
}