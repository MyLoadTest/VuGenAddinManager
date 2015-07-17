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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Control = System.Windows.Forms.Control;

//// ReSharper disable once CheckNamespace - SD 5.0 Compatibility
namespace MyLoadTest.VuGenAddInManager.Compatibility
{
    /// <summary>
    /// A custom Windows Forms Host implementation.
    /// Hopefully fixes SD-1842 - ArgumentException in SetActiveControlInternal (WindowsFormsHost.RestoreFocusedChild)
    /// </summary>
    public class CustomWindowsFormsHost : HwndHost
    {
        // Interactions of the MS WinFormsHost:
        // IME
        // Font sync
        // Property sync
        // Background sync (rendering bitmaps!)
        // Access keys
        // Tab Navigation
        // Save/Restore focus for app switch
        // Size feedback (WinForms control tells WPF desired size)
        // Focus enter/leave - validation events
        // ...

        // We don't need most of that.

        //// Bugs in our implementation:
        ////  - Slight background color mismatch in project options
        private static class Win32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetFocus();

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsChild(IntPtr hWndParent, IntPtr hwnd);

            [DllImport("user32.dll")]
            internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            internal static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
        }

        #region Remember/RestoreFocusedChild

        private sealed class HostNativeWindow : NativeWindow
        {
            private readonly CustomWindowsFormsHost _host;

            public HostNativeWindow(CustomWindowsFormsHost host)
            {
                this._host = host;
            }

            protected override void WndProc(ref Message m)
            {
                //// ReSharper disable once InconsistentNaming - WinApi constant
                const int WM_ACTIVATEAPP = 0x1C;

                if (m.Msg == WM_ACTIVATEAPP)
                {
                    if (m.WParam == IntPtr.Zero)
                    {
                        // The window is being deactivated:
                        // If a WinForms control within this host has focus, remember it.
                        var focus = Win32.GetFocus();
                        if (focus == _host.Handle || Win32.IsChild(_host.Handle, focus))
                        {
                            _host.RememberActiveControl();
                            _host.Log("Window deactivated; RememberActiveControl(): " + _host._savedActiveControl);
                        }
                        else
                        {
                            _host.Log("Window deactivated; but focus not within WinForms");
                        }
                    }
                    else
                    {
                        // The window is being activated.
                        _host.Log("Window activated");
                        _host.Dispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            new Action(_host.RestoreActiveControl));
                    }
                }

                base.WndProc(ref m);
            }

            protected override void OnThreadException(Exception e)
            {
                System.Windows.Forms.Application.OnThreadException(e);
            }
        }

        private HostNativeWindow _hostNativeWindow;
        private Control _savedActiveControl;

        private void RememberActiveControl()
        {
            _savedActiveControl = _container.ActiveControl;
        }

        private void RestoreActiveControl()
        {
            if (_savedActiveControl != null)
            {
                Log("RestoreActiveControl(): " + _savedActiveControl);
                _savedActiveControl.Focus();
                _savedActiveControl = null;
            }
        }

        #endregion

        #region Container

        private sealed class HostedControlContainer : ContainerControl
        {
            private Control _child;

            protected override void OnHandleCreated(EventArgs e)
            {
                //// ReSharper disable once InconsistentNaming - WinApi constant
                const int WM_UPDATEUISTATE = 0x0128;

                //// ReSharper disable once InconsistentNaming - WinApi constant
                const int UISF_HIDEACCEL = 2;

                //// ReSharper disable once InconsistentNaming - WinApi constant
                const int UISF_HIDEFOCUS = 1;

                //// ReSharper disable once InconsistentNaming - WinApi constant
                const int UIS_SET = 1;

                base.OnHandleCreated(e);

                Win32.SendMessage(
                    this.Handle,
                    WM_UPDATEUISTATE,
                    new IntPtr(UISF_HIDEACCEL | UISF_HIDEFOCUS | (UIS_SET << 16)),
                    IntPtr.Zero);
            }

            public Control Child
            {
                get
                {
                    return _child;
                }

                set
                {
                    if (_child != null)
                    {
                        this.Controls.Remove(_child);
                    }

                    _child = value;
                    if (value != null)
                    {
                        value.Dock = DockStyle.Fill;
                        this.Controls.Add(value);
                    }
                }
            }
        }

        #endregion

        private readonly HostedControlContainer _container;

        #region Constructors

        /// <summary>
        /// Creates a new CustomWindowsFormsHost instance.
        /// </summary>
        public CustomWindowsFormsHost()
        {
            this._container = new HostedControlContainer();
            Init();
        }

        /// <summary>
        /// Creates a new CustomWindowsFormsHost instance that allows hosting controls
        /// from the specified AppDomain.
        /// </summary>
        public CustomWindowsFormsHost(AppDomain childDomain)
        {
            var type = typeof(HostedControlContainer);
            this._container =
                (HostedControlContainer)childDomain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            Init();
        }

        private void Init()
        {
            this.EnableFontInheritance = true;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            Log("Instance created");
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Log("OnLoaded()");
            SetFont();
            if (_hwndParent.Handle != IntPtr.Zero && _hostNativeWindow != null)
            {
                if (_hostNativeWindow.Handle == IntPtr.Zero)
                {
                    _hostNativeWindow.AssignHandle(_hwndParent.Handle);
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Log("OnUnloaded()");
            if (_hostNativeWindow != null)
            {
                _savedActiveControl = null;
                _hostNativeWindow.ReleaseHandle();
            }
        }

        #endregion

        #region Font Synchronization

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == TextBlock.FontFamilyProperty || e.Property == TextBlock.FontSizeProperty)
            {
                SetFont();
            }
        }

        public bool EnableFontInheritance
        {
            get;
            set;
        }

        private void SetFont()
        {
            if (!EnableFontInheritance)
            {
                return;
            }

            var fontFamily = TextBlock.GetFontFamily(this).Source;
            var fontSize = (float)(TextBlock.GetFontSize(this) * (72.0 / 96.0));
            _container.Font = new System.Drawing.Font(fontFamily, fontSize, System.Drawing.FontStyle.Regular);
        }

        #endregion

        public Control Child
        {
            get
            {
                return _container.Child;
            }

            set
            {
                _container.Child = value;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _container.Size = new System.Drawing.Size((int)finalSize.Width, (int)finalSize.Height);
            return finalSize;
        }

        private HandleRef _hwndParent;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            Log("BuildWindowCore");
            if (_hostNativeWindow != null)
            {
                _hostNativeWindow.ReleaseHandle();
            }
            else
            {
                _hostNativeWindow = new HostNativeWindow(this);
            }

            this._hwndParent = hwndParent;
            _hostNativeWindow.AssignHandle(hwndParent.Handle);

            var childHandle = _container.Handle;
            Win32.SetParent(childHandle, hwndParent.Handle);
            return new HandleRef(_container, childHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            Log("DestroyWindowCore");
            _hostNativeWindow.ReleaseHandle();
            _savedActiveControl = null;
            _hwndParent = default(HandleRef);
        }

        protected override void Dispose(bool disposing)
        {
            Log("Dispose (disposing=" + disposing + ")");
            base.Dispose(disposing);
            if (disposing)
            {
                _container.Dispose();
            }
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0007: // WM_SETFOCUS
                case 0x0021: // WM_MOUSEACTIVATE

                    // Give the WindowsFormsHost logical focus:
                    DependencyObject focusScope = this;
                    while (focusScope != null && !FocusManager.GetIsFocusScope(focusScope))
                    {
                        focusScope = VisualTreeHelper.GetParent(focusScope);
                    }

                    if (focusScope != null)
                    {
                        FocusManager.SetFocusedElement(focusScope, this);
                    }

                    break;
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

#if DEBUG
        private static int _hostCount;
        private readonly int _instanceId = System.Threading.Interlocked.Increment(ref _hostCount);
#endif

        [Conditional("DEBUG")]
        private void Log(string text)
        {
#if DEBUG
            Debug.WriteLine("CustomWindowsFormsHost #{0}: {1}", _instanceId, text);
#endif
        }
    }
}