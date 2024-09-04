using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification; // For TaskbarIcon
using WpfApplication = System.Windows.Application; // Alias for System.Windows.Application

namespace WpfApp1.Components
{
    public class ToggleSwitchHandler
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
        private TaskbarIcon taskbarIcon;

        private ToggleButton _toggleSwitch;

        public ToggleSwitchHandler(ToggleButton toggleSwitch)
        {
            _toggleSwitch = toggleSwitch;
            InitializeMutex();
            InitializeCleware();
            InitializeTaskbarIcon();
        }

        private void InitializeMutex()
        {
            appMutex = new Mutex(true, "MO_Cleware_SwitchApp", out bool isNewInstance);
            if (!isNewInstance)
            {
                System.Windows.MessageBox.Show("Another instance is already running.", "Instance Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WpfApplication.Current.Shutdown();
            }
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

            _toggleSwitch.IsChecked = currentState == 1;
        }

        private void InitializeTaskbarIcon()
        {
            taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "MO-Cleware App",
                Visibility = Visibility.Hidden
            };
            taskbarIcon.TrayMouseDoubleClick += TaskbarIcon_DoubleClick;
        }

        public void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 1);
            if (result <= 0)
            {
                System.Windows.MessageBox.Show("Failed to turn on the switch.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _toggleSwitch.IsChecked = false;
            }
            else
            {
                currentState = 1;
            }
        }

        public void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 0);
            if (result <= 0)
            {
                System.Windows.MessageBox.Show("Failed to turn off the switch.", "Switch Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _toggleSwitch.IsChecked = true;
            }
            else
            {
                currentState = 0;
            }
        }

        private void TaskbarIcon_DoubleClick(object? sender, RoutedEventArgs e)
        {
            WpfApplication.Current.MainWindow.Show();
            WpfApplication.Current.MainWindow.WindowState = WindowState.Normal;
            taskbarIcon.Visibility = Visibility.Hidden;
        }

        public void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WpfApplication.Current.MainWindow.WindowState == WindowState.Minimized)
            {
                WpfApplication.Current.MainWindow.Hide();
                taskbarIcon.Visibility = Visibility.Visible;
            }
        }

        public void DisposeResources()
        {
            taskbarIcon.Dispose();
            if (cwPointer != IntPtr.Zero)
            {
                FCWCloseCleware(cwPointer);
                FCWUnInitObject(cwPointer);
            }
            appMutex?.ReleaseMutex();
            appMutex?.Dispose();
        }
    }
}
