using System;
using System.Windows;
using System.Windows.Interop;
using ImgurSniper.Libraries.Native;
using System.Windows.Forms;
using System.Drawing;

namespace ImgurSniper.Libraries.Helper {
    public static class WindowHelper {

        //Size of current Mouse Location screen
        public static Rectangle Screen
            => System.Windows.Forms.Screen.FromPoint(Cursor.Position).Bounds;

        //Size of whole Screen Array
        public static Rectangle AllScreens => SystemInformation.VirtualScreen;


        //Position Window correctly
        public static void Position(Window window) {
            Rectangle size = ConfigHelper.AllMonitors ? AllScreens : Screen;

            window.Left = size.Left;
            window.Top = size.Top;
            window.Width = size.Width;
            window.Height = size.Height;
        }

        public static void WindowLoaded(Window window) {

            //Activate & Focus Window
            window.Activate();
            window.Focus();

            //Hide in Alt + Tab Switcher View
            WindowInteropHelper wndHelper = new WindowInteropHelper(window);

            int exStyle = (int)NativeMethods.GetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle);

            exStyle |= (int)NativeMethods.ExtendedWindowStyles.WsExToolwindow;
            NativeMethods.SetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GwlExstyle, (IntPtr)exStyle);
        }
    }
}
