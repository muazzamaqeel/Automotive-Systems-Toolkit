using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;
using WpfApplication = System.Windows.Application;

namespace MO_Cleware_SwitchApp
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReleaseMutex(IntPtr hMutex);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FCWInitObject();

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FCWOpenCleware(IntPtr cw);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FCWGetSwitch(IntPtr cw, int switchIndex, int switchId);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FCWSetSwitch(IntPtr cw, int switchIndex, int switchId, int state);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FCWCloseCleware(IntPtr cw);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FCWUnInitObject(IntPtr cw);

        private const int WM_SETICON = 0x80;
        private IntPtr cwPointer;
        private int currentState = 0;
        private const int switchIndex = 0;
        private const int switchId = 0x10;
        private Mutex appMutex;
        private NotifyIcon notifyIcon;
        private System.Threading.Timer deviceCheckTimer; 

        public MainWindow()
        {
            appMutex = new Mutex(true, "MO_Cleware_SwitchApp", out bool isNewInstance);

            if (!isNewInstance)
            {
                System.Windows.MessageBox.Show("Another instance is already running.", "Instance Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown();
                return;
            }

            InitializeComponent();
            InitializeNotifyIcon();
            this.StateChanged += MainWindow_StateChanged;
            InitializeCleware();

            // Start the timer to check the device connection status every 2 seconds
            deviceCheckTimer = new System.Threading.Timer(CheckDeviceConnection, null, 2000, 2000);
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iconOff.ico")),
                Visible = true,
                Text = "MO-Cleware App"
            };
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void InitializeCleware()
        {
            cwPointer = FCWInitObject();

            int devCnt = FCWOpenCleware(cwPointer);
            if (devCnt <= 0)
            {
                System.Windows.MessageBox.Show("No Cleware devices found. Please connect the device and try again.", "Device Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown();
                return;
            }

            currentState = FCWGetSwitch(cwPointer, switchIndex, switchId);
            if (currentState == -1)
            {
                System.Windows.MessageBox.Show("Failed to retrieve the current switch state.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown();
                return;
            }

            toggleSwitch.IsChecked = currentState == 1;
            UpdateTrayIcon();
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 1);
            if (result <= 0)
            {
                toggleSwitch.IsChecked = false;
            }
            else
            {
                currentState = 1;
                UpdateTrayIcon();
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 0);
            if (result <= 0)
            {
                toggleSwitch.IsChecked = true;
            }
            else
            {
                currentState = 0;
                UpdateTrayIcon();
            }
        }

        private void UpdateTrayIcon()
        {
            string iconPath = currentState == 1
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico")
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iconOff.ico");

            ChangeWindowIcon(iconPath);
            notifyIcon.Icon = new Icon(iconPath);
            notifyIcon.Visible = true;
        }

        private void ChangeWindowIcon(string iconPath)
        {
            IntPtr iconHandle = System.Drawing.Icon.ExtractAssociatedIcon(iconPath).Handle;
            SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle, WM_SETICON, (IntPtr)0, iconHandle);
            SendMessage(new System.Windows.Interop.WindowInteropHelper(this).Handle, WM_SETICON, (IntPtr)1, iconHandle);
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void CheckDeviceConnection(object state)
        {
            // Check if the device is still connected
            if (cwPointer == IntPtr.Zero || FCWGetSwitch(cwPointer, switchIndex, switchId) == -1)
            {
                deviceCheckTimer?.Dispose(); // Stop the timer
                WpfApplication.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show("You disconnected the device.", "Device Disconnected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    WpfApplication.Current.Shutdown(); // Close the application
                });
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            notifyIcon?.Dispose();
            deviceCheckTimer?.Dispose();
            if (cwPointer != IntPtr.Zero)
            {
                FCWCloseCleware(cwPointer);
                FCWUnInitObject(cwPointer);
            }
            appMutex?.ReleaseMutex();
            appMutex?.Dispose();
            base.OnClosed(e);
        }
    }
}
