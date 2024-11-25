using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;

namespace IPToggle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadNetworkConnections();
        }

        private void LoadNetworkConnections()
        {
            var networkConnections = GetNetworkInterfaces();
            NetworkConnectionsDataGrid.ItemsSource = networkConnections;
        }

        private List<NetworkConnectionInfo> GetNetworkInterfaces()
        {
            List<NetworkConnectionInfo> networkInfoList = new List<NetworkConnectionInfo>();

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                networkInfoList.Add(new NetworkConnectionInfo
                {
                    Name = nic.Name,
                    Description = nic.Description,
                    Status = nic.OperationalStatus.ToString(),
                    Speed = nic.Speed / 1_000_000 + " Mbps",
                    Type = nic.NetworkInterfaceType.ToString(),
                    IPs = string.Join(", ", nic.GetIPProperties().UnicastAddresses
                        .Select(ip => ip.Address.ToString())),
                    ToggleIPv4Command = new RelayCommand(() => ToggleIPProtocol(nic.Name, "ipv4")),
                    ToggleIPv6Command = new RelayCommand(() => ToggleIPProtocol(nic.Name, "ipv6"))
                });
            }

            return networkInfoList;
        }

        private void ToggleIPProtocol(string adapterName, string protocol)
        {
            try
            {
                string command = protocol.ToLower() switch
                {
                    "ipv4" => $"netsh interface ipv4 set interface \"{adapterName}\" admin=disabled",
                    "ipv6" => $"netsh interface ipv6 set interface \"{adapterName}\" admin=disabled",
                    _ => throw new InvalidOperationException("Invalid protocol")
                };

                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    MessageBox.Show($"{protocol.ToUpper()} toggled successfully for adapter {adapterName}.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling {protocol.ToUpper()} for adapter {adapterName}: {ex.Message}");
            }
        }
    }

    public class NetworkConnectionInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Speed { get; set; }
        public string Type { get; set; }
        public string IPs { get; set; }
        public ICommand ToggleIPv4Command { get; set; }
        public ICommand ToggleIPv6Command { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}