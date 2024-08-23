using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms; // Added for NotifyIcon
using System.Drawing; // Added for Icon
using WpfApplication = System.Windows.Application; // Alias for System.Windows.Application

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

        private IntPtr cwPointer;
        private int currentState = 0;
        private const int switchIndex = 0;
        private const int switchId = 0x10;
        private Mutex appMutex;

        private NotifyIcon notifyIcon = new NotifyIcon();  // Initialized here


        public MainWindow()
        {
            // Initialize the mutex
            appMutex = new Mutex(true, "MO_Cleware_SwitchApp", out bool isNewInstance);

            if (!isNewInstance)
            {
                System.Windows.MessageBox.Show("Another instance is already running.", "Instance Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown(); // Exit the application
                return;
            }

            InitializeComponent();

            // Initialize the NotifyIcon (System Tray Icon)
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon("icon.ico"); // Referencing the icon file by name
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            notifyIcon.Text = "MO-Cleware App";

            this.StateChanged += MainWindow_StateChanged;

            InitializeCleware();
        }

        private void InitializeCleware()
        {
            cwPointer = FCWInitObject();

            int devCnt = FCWOpenCleware(cwPointer);
            if (devCnt <= 0)
            {
                System.Windows.MessageBox.Show("No Cleware devices found. Please connect the device and try again.", "Device Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown(); // Use alias for WPF Application
                return;
            }

            currentState = FCWGetSwitch(cwPointer, switchIndex, switchId);
            if (currentState == -1)
            {
                System.Windows.MessageBox.Show("Failed to retrieve the current switch state.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown(); // Use alias for WPF Application
                return;
            }

            toggleSwitch.IsChecked = currentState == 1;
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 1);
            if (result <= 0)
            {
                System.Windows.MessageBox.Show("Failed to turn on the switch.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                toggleSwitch.IsChecked = false; // revert the state
            }
            else
            {
                currentState = 1;
            }
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 0);
            if (result <= 0)
            {
                System.Windows.MessageBox.Show("Failed to turn off the switch.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                toggleSwitch.IsChecked = true; // revert the state
            }
            else
            {
                currentState = 0;
            }
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                notifyIcon.Visible = false;
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            notifyIcon.Dispose();
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
