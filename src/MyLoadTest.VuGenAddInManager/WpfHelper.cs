using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MyLoadTest.VuGenAddInManager
{
    //// WpfHelper is borrowed from Omnifactotum.Wpf (being developed).
    internal static class WpfHelper
    {
        #region Public Methods

        public static bool IsInDesignMode()
        {
            try
            {
                return (bool)DesignerProperties
                    .IsInDesignModeProperty
                    .GetMetadata(typeof(DependencyObject))
                    .DefaultValue;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}