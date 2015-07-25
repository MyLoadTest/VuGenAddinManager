using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MyLoadTest.VuGenAddInManager
{
    //// This class is borrowed from Omnifactotum.Wpf which is being developed and not yet released at the moment

    internal static class ExtraWindowStyles
    {
        #region Constants and Fields

        public static readonly DependencyProperty CanMinimizeProperty =
            DependencyProperty.RegisterAttached(
                "CanMinimize",
                typeof(bool),
                typeof(ExtraWindowStyles),
                new UIPropertyMetadata(true, OnCanMinimizeChanged));

        public static readonly DependencyProperty CanMaximizeProperty =
            DependencyProperty.RegisterAttached(
                "CanMaximize",
                typeof(bool),
                typeof(ExtraWindowStyles),
                new UIPropertyMetadata(true, OnCanMaximizeChanged));

        public static readonly DependencyProperty HasSystemMenuProperty =
            DependencyProperty.RegisterAttached(
                "HasSystemMenu",
                typeof(bool),
                typeof(ExtraWindowStyles),
                new UIPropertyMetadata(true, OnHasSystemMenuChanged));

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_FRAMECHANGED = 0x0020;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOACTIVATE = 0x0010;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOMOVE = 0x0002;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOOWNERZORDER = 0x0200;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOREPOSITION = 0x0200;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOSIZE = 0x0001;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int SWP_NOZORDER = 0x0004;

        //// ReSharper disable once InconsistentNaming - WinAPI import
        private const int GWL_STYLE = -16;

        private const string User32Dll = "user32.dll";

        #endregion

        #region WindowStyles Enumeration

        [Flags]
        private enum WindowStyles : uint
        {
            //// ReSharper disable once InconsistentNaming - WinAPI import
            WS_SYSMENU = 0x80000,

            //// ReSharper disable once InconsistentNaming - WinAPI import
            WS_MINIMIZEBOX = 0x20000,

            //// ReSharper disable once InconsistentNaming - WinAPI import
            WS_MAXIMIZEBOX = 0x10000,
        }

        #endregion

        #region Public Methods

        public static bool GetCanMinimize(Window obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(CanMinimizeProperty);
        }

        public static void SetCanMinimize(Window obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(CanMinimizeProperty, value);
        }

        public static bool GetCanMaximize(Window obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(CanMaximizeProperty);
        }

        public static void SetCanMaximize(Window obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(CanMaximizeProperty, value);
        }

        public static bool GetHasSystemMenu(Window obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(HasSystemMenuProperty);
        }

        public static void SetHasSystemMenu(Window obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(HasSystemMenuProperty, value);
        }

        #endregion

        #region Private Methods: WinAPI Imports and Helpers

        [DllImport(User32Dll, EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport(User32Dll, EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport(User32Dll, EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport(User32Dll, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport(User32Dll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);
        }

        //// ReSharper disable once UnusedMethodReturnValue.Local - WinAPI wrapper. keeping signature
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        private static void ResetWindowStyle(Window window, WindowStyles styles, bool set)
        {
            const int Flags = SWP_FRAMECHANGED | SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOOWNERZORDER | SWP_NOREPOSITION
                | SWP_NOSIZE | SWP_NOZORDER;

            var wih = new WindowInteropHelper(window);
            var style = (WindowStyles)GetWindowLongPtr(wih.EnsureHandle(), GWL_STYLE);

            if (set)
            {
                style |= styles;
            }
            else
            {
                style &= ~styles;
            }

            SetWindowLongPtr(wih.Handle, GWL_STYLE, (IntPtr)style);
            SetWindowPos(wih.Handle, IntPtr.Zero, 0, 0, 0, 0, Flags);
        }

        #endregion

        #region Private Methods: Regular

        private static void OnCanMaximizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            ResetWindowStyle(window, WindowStyles.WS_MAXIMIZEBOX, (bool)args.NewValue);
        }

        private static void OnCanMinimizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            ResetWindowStyle(window, WindowStyles.WS_MINIMIZEBOX, (bool)args.NewValue);
        }

        private static void OnHasSystemMenuChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var window = obj as Window;
            if (window == null)
            {
                return;
            }

            ResetWindowStyle(window, WindowStyles.WS_SYSMENU, (bool)args.NewValue);
        }

        #endregion
    }
}