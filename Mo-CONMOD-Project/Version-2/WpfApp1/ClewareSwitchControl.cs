using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WpfApp1
{
    public class ClewareSwitchControl : IDisposable
    {
        private IntPtr cwPointer;
        private int switchIndex = 0;
        private int switchId = 0x10; // Assume the first device is the switch device

        // DLL Imports
        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FCWInitObject();

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FCWOpenCleware(IntPtr cwPointer);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FCWSetSwitch(IntPtr cwPointer, int switchIndex, int switchId, int state);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FCWCloseCleware(IntPtr cwPointer);

        [DllImport("USBaccessX64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FCWUnInitObject(IntPtr cwPointer);

        public ClewareSwitchControl()
        {
            // Set the path to the DLL explicitly
            string dllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "USBaccessX64.dll");
            IntPtr hModule = LoadLibrary(dllPath);
            if (hModule == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Failed to load DLL from path: {dllPath}");
            }

            // Initialize Cleware object
            cwPointer = FCWInitObject();
            if (cwPointer == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to initialize Cleware object");
            }

            // Open Cleware devices
            int devCnt = FCWOpenCleware(cwPointer);
            if (devCnt <= 0)
            {
                throw new InvalidOperationException("No Cleware devices found");
            }
        }

        // Method to turn on the switch
        public void TurnOnSwitch()
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 1); // 1 = ON
            if (result > 0)
            {
                MessageBox.Show("Switch turned ON successfully", "Switch Control", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to turn ON the switch, error code: {result}", "Switch Control", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to turn off the switch
        public void TurnOffSwitch()
        {
            int result = FCWSetSwitch(cwPointer, switchIndex, switchId, 0); // 0 = OFF
            if (result > 0)
            {
                MessageBox.Show("Switch turned OFF successfully", "Switch Control", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to turn OFF the switch, error code: {result}", "Switch Control", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Clean up resources
        public void Dispose()
        {
            FCWCloseCleware(cwPointer);
            FCWUnInitObject(cwPointer);
        }

        // Import LoadLibrary from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }
}
