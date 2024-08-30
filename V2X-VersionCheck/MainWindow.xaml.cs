using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace V2X_VersionCheck
{
    public partial class MainWindow : Window
    {
        private SerialPort _wucSerialPort;
        private SerialPort _v2xSerialPort;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _catCommandSent = false;
        private string _versionOutput = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWUCSerialPort();
            LogToTextBox("Application initialized.");
        }

        private void InitializeWUCSerialPort()
        {
            _wucSerialPort = new SerialPort
            {
                PortName = "COM3", // Adjust this to your WUC COM port
                BaudRate = 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 5000
            };

            _wucSerialPort.DataReceived += WUCSerialPort_DataReceived;
            LogToTextBox("WUC Serial Port initialized.");
        }

        private void InitializeV2XSerialPort()
        {
            _v2xSerialPort = new SerialPort
            {
                PortName = "COM9", // Adjust this to the V2X component's COM port
                BaudRate = 115200,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 5000
            };

            _v2xSerialPort.DataReceived += V2XSerialPort_DataReceived;
            LogToTextBox("V2X Serial Port initialized.");
        }

        private async void CheckVersionButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => ProgressBar.IsIndeterminate = true);
            LogToTextBox("Check Version button clicked. Starting WUC connection...");

            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => StartWUCConnection(_cancellationTokenSource.Token));
        }

        private void StartWUCConnection(CancellationToken cancellationToken)
        {
            try
            {
                Dispatcher.Invoke(() => LogToTextBox("Attempting to open WUC Serial Port..."));
                _wucSerialPort.Open();
                Dispatcher.Invoke(() => LogToTextBox("WUC Serial Port opened."));

                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(100); // Sleep briefly to avoid tight loop
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in StartWUCConnection: {ex.Message}"));
            }
        }

        private void WUCSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!_wucSerialPort.IsOpen) return; // Ensure the port is open

                string data = _wucSerialPort.ReadExisting();
                Dispatcher.Invoke(() => LogToTextBox($"Data received from WUC Serial Port: {data.Trim()}"));

                // Send the "r" command immediately upon receiving any data
                SendRestartCommand();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in WUCSerialPort_DataReceived: {ex.Message}"));
            }
        }

        private void SendRestartCommand()
        {
            try
            {
                _wucSerialPort.WriteLine("r"); // Send the restart command
                Dispatcher.Invoke(() =>
                {
                    WUCCheckBox.IsChecked = true; // Tick the WUC checkbox
                    LogToTextBox("Restart command sent. WUC Checkbox ticked.");

                    // Close WUC and open V2X connection after a short delay
                    Task.Delay(2000).ContinueWith(t =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _wucSerialPort.Close();
                            LogToTextBox("WUC Serial Port closed. Attempting to open V2X connection...");
                            StartV2XConnection();
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in SendRestartCommand: {ex.Message}"));
            }
        }

        private void StartV2XConnection()
        {
            try
            {
                if (_v2xSerialPort != null && _v2xSerialPort.IsOpen)
                {
                    LogToTextBox("V2X Serial Port is already open.");
                }
                else
                {
                    InitializeV2XSerialPort();
                    _v2xSerialPort.Open();
                    LogToTextBox("V2X Serial Port opened.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                LogToTextBox($"Access denied to V2X Serial Port: {ex.Message}");
            }
            catch (IOException ex)
            {
                LogToTextBox($"IO Exception on V2X Serial Port: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogToTextBox($"Error in StartV2XConnection: {ex.Message}");
            }
        }

        private void V2XSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!_v2xSerialPort.IsOpen) return;

                string data = _v2xSerialPort.ReadExisting();
                _versionOutput += data;

                Dispatcher.Invoke(() =>
                {
                    LogTextBox.AppendText(data);
                    LogTextBox.ScrollToEnd();

                    if (_catCommandSent && data.Contains("root@autotalks:~#"))
                    {
                        ParseVersionOutput(_versionOutput);
                        _versionOutput = string.Empty;
                        _catCommandSent = false;
                    }
                    else if (!_catCommandSent && data.Contains("root@autotalks:~#"))
                    {
                        LogToTextBox("V2X is ready. Waiting 1 second before sending cat /etc/version command...");
                        Task.Delay(1000).ContinueWith(t =>
                        {
                            Dispatcher.Invoke(() =>
                            {
                                SendCatCommand();
                                _catCommandSent = true;
                            });
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in V2XSerialPort_DataReceived: {ex.Message}"));
            }
        }

        private void SendCatCommand()
        {
            try
            {
                _v2xSerialPort.WriteLine("cat /etc/version");
                Dispatcher.Invoke(() => LogToTextBox("Sent 'cat /etc/version' command."));
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in SendCatCommand: {ex.Message}"));
            }
        }

        private void ParseVersionOutput(string versionOutput)
        {
            try
            {
                string[] lines = versionOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                string line2 = "Undefined";
                string line5 = string.Empty;
                string line6 = string.Empty;

                // Find the specific lines you want
                foreach (var line in lines)
                {
                    if (line.Trim().Equals("PROD") || line.Trim().Equals("DEV"))
                    {
                        line2 = line.Trim();
                    }
                    else if (line.Trim().StartsWith("CPM_TAG"))
                    {
                        line5 = line.Trim();
                    }
                    else if (line.Trim().StartsWith("GTC_TAG"))
                    {
                        line6 = line.Trim();
                    }
                }

                // Display the found lines
                if (!string.IsNullOrEmpty(line2) && !string.IsNullOrEmpty(line5) && !string.IsNullOrEmpty(line6))
                {
                    LogToTextBox($"Parsed Line 2: {line2}");
                    LogToTextBox($"Parsed Line 5: {line5}");
                    LogToTextBox($"Parsed Line 6: {line6}");
                    UpdateCommandOutputBox(line2, line5, line6);
                }
                else
                {
                    Dispatcher.Invoke(() => LogToTextBox("Error: Could not find the required lines."));
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => LogToTextBox($"Error in ParseVersionOutput: {ex.Message}"));
            }
        }

        private void UpdateCommandOutputBox(string line2, string line5, string line6)
        {
            Dispatcher.Invoke(() =>
            {
                CommandOutputTextBox.Text = $"Line 2: {line2}\nLine 5: {line5}\nLine 6: {line6}";
                LogToTextBox("Command output updated in the right box.");
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            if (_v2xSerialPort?.IsOpen == true)
            {
                _v2xSerialPort.Close();
            }

            base.OnClosing(e);
        }

        private void LogToTextBox(string message)
        {
            LogTextBox.AppendText(message + "\n");
            LogTextBox.ScrollToEnd();
        }
    }
}
