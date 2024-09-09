using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1;
using MO_TERMINAL;
using WpfApp1.Components.TracingWindow;
using System.Collections.Generic;
using System.Security.Policy;

namespace MO_TERMINAL
{
    public partial class TracingWindow : Window
    {
        private SerialPortManager serialPortManager = new SerialPortManager();
        private ClewareSwitchControl switchControl;
        private ToggleSwitchHandler _toggleSwitchHandler; // Declare ToggleSwitchHandler

        // Dictionary to track connected ports
        private Dictionary<int, string> _connectedPorts = new Dictionary<int, string>();

        public TracingWindow()
        {
            InitializeComponent(); // Initialize the components first

            // Check if toggleSwitch is available and then initialize the handler
            if (toggleSwitch != null)
            {
                _toggleSwitchHandler = new ToggleSwitchHandler(toggleSwitch);

                // Link the ToggleButton's Checked and Unchecked events after handler initialization
                toggleSwitch.Checked += ToggleSwitch_Checked;
                toggleSwitch.Unchecked += ToggleSwitch_Unchecked;
            }
            else
            {
                MessageBox.Show("Toggle switch is not initialized.");
            }

            // Handle window state changes
            this.StateChanged += MainWindow_StateChanged;

            // Load COM Ports
            LoadCOMPorts();
            StartAutoDetection();
            switchControl = new ClewareSwitchControl();
        }

        // Forward ToggleSwitch_Checked to ToggleSwitchHandler
        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            _toggleSwitchHandler?.ToggleSwitch_Checked(sender, e);
        }

        // Forward ToggleSwitch_Unchecked to ToggleSwitchHandler
        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            _toggleSwitchHandler?.ToggleSwitch_Unchecked(sender, e);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (_toggleSwitchHandler != null)
            {
                _toggleSwitchHandler.MainWindow_StateChanged(sender, e);
            }
        }

        private void LoadCOMPorts()
        {
            // Clear the existing list first
            COMPortList.Items.Clear();

            // Get available COM ports
            string[] ports = serialPortManager.GetAvailablePorts();

            // Add each port to the ListBox (COMPortList)
            foreach (string port in ports)
            {
                COMPortList.Items.Add(port);
            }
        }

        private void StartAutoDetection()
        {
            int maxFrames = 4;

            for (int i = 0; i < maxFrames; i++)
            {
                if (i < COMPortList.Items.Count)
                {
                    string? port = COMPortList.Items[i]?.ToString();
                    if (!string.IsNullOrEmpty(port))
                    {
                        int selectedFrame = i;
                        int baudRate = 115200;

                        Task.Run(async () =>
                        {
                            bool connected = await serialPortManager.ConnectAsync(selectedFrame, port, baudRate);
                            if (connected)
                            {
                                Dispatcher.Invoke(() => COMPortList.Items.Remove(port));

                                serialPortManager.RegisterDataReceivedHandler(selectedFrame, (s, args) =>
                                {
                                    string data = serialPortManager.ReadExisting(selectedFrame);
                                    string dataType = serialPortManager.IdentifyDataType(data);
                                    Dispatcher.Invoke(() => DisplayData(selectedFrame, data, dataType, port));
                                });
                            }
                        });
                    }
                }
            }
        }

        private void DisplayData(int frameIndex, string data, string dataType, string portName)
        {
            TextBox? frameDataTextBox = null;
            ScrollViewer? frameScrollViewer = null;
            TabItem? frameTabItem = null;

            // Match the frame index to the appropriate frame
            switch (frameIndex)
            {
                case 0:
                    frameDataTextBox = Frame1Data;
                    frameScrollViewer = Frame1ScrollViewer;
                    frameTabItem = FrameTabControl.Items[0] as TabItem;
                    break;
                case 1:
                    frameDataTextBox = Frame2Data;
                    frameScrollViewer = Frame2ScrollViewer;
                    frameTabItem = FrameTabControl.Items[1] as TabItem;
                    break;
                case 2:
                    frameDataTextBox = Frame3Data;
                    frameScrollViewer = Frame3ScrollViewer;
                    frameTabItem = FrameTabControl.Items[2] as TabItem;
                    break;
                case 3:
                    frameDataTextBox = Frame4Data;
                    frameScrollViewer = Frame4ScrollViewer;
                    frameTabItem = FrameTabControl.Items[3] as TabItem;
                    break;
            }

            if (frameDataTextBox != null)
            {
                // Append data to the appropriate frame's TextBox
                frameDataTextBox.AppendText(data);
                ScrollToBottom(frameScrollViewer!);

                // Update the TabItem header based on data type and port name
                if (!string.IsNullOrEmpty(data) && frameTabItem != null)
                {
                    // Logic to identify data type
                    if (data.Contains("W01") || data.Contains("WUC"))
                    {
                        frameTabItem.Header = $"WUC - {portName}";
                    }
                    else if (data.Contains("ECALL_STATE") || data.Contains("NAD") || data.Contains("Enter HSM BL") || data.Contains("Set JTAG Done"))
                    {
                        frameTabItem.Header = $"NAD - {portName}";
                    }
                    else if (data.Contains("V2X") || data.Contains("SEQ_A_OK.") || data.Contains("SEQ_A_OK"))
                    {
                        frameTabItem.Header = $"V2X - {portName}";
                    }
                    // You can extend this logic to handle IOC or any other data type
                }
            }
        }

        private void ScrollToBottom(ScrollViewer scrollViewer)
        {
            scrollViewer?.ScrollToEnd();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCOMPorts();
            StartAutoDetection();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure you have selected the COM port and frame
            if (COMPortList.SelectedItem != null && FrameSelector.SelectedIndex != -1 && BaudRateSelector.SelectedItem != null)
            {
                string? selectedPort = COMPortList.SelectedItem?.ToString();

                // Correct way to get selected baud rate from BaudRateSelector ComboBox
                if (!string.IsNullOrEmpty(selectedPort) && int.TryParse((BaudRateSelector.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int baudRate))
                {
                    int selectedFrame = FrameSelector.SelectedIndex;

                    // Connect to the selected port and frame
                    bool connected = await serialPortManager.ConnectAsync(selectedFrame, selectedPort, baudRate);

                    if (connected)
                    {
                        // Remove the selected port from the ListBox once connected
                        Dispatcher.InvokeAsync(() => COMPortList.Items.Remove(selectedPort));

                        // Register a handler to listen to the incoming data
                        serialPortManager.RegisterDataReceivedHandler(selectedFrame, (s, args) =>
                        {
                            var task = Task.Run(() =>
                            {
                                string data = serialPortManager.ReadExisting(selectedFrame);
                                string dataType = serialPortManager.IdentifyDataType(data);
                                Dispatcher.InvokeAsync(() => DisplayData(selectedFrame, data, dataType, selectedPort));
                            });
                        });

                        // Store the connected port in the dictionary
                        _connectedPorts[selectedFrame] = selectedPort;

                        MessageBox.Show($"Connected to {selectedPort}", "Connection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Error connecting to {selectedPort}.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid baud rate selected.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a COM port, frame, and baud rate.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // In order to refresh the COM ports when disconnecting a serial connection
        private void RefreshCOMPorts()
        {
            LoadCOMPorts();
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem? selectedTab = FrameTabControl.SelectedItem as TabItem;
            int selectedFrame = FrameTabControl.SelectedIndex;

            if (serialPortManager.IsConnected(selectedFrame))
            {
                serialPortManager.Disconnect(selectedFrame);
                ClearFrameData(selectedFrame);

                if (_connectedPorts.ContainsKey(selectedFrame))
                {
                    _connectedPorts.Remove(selectedFrame); // Remove the port from the dictionary
                }

                if (selectedTab != null)
                {
                    string portName = selectedTab.Header.ToString().Split('-').Last().Trim();
                    selectedTab.Header = $"Frame {selectedFrame + 1}";
                    MessageBox.Show($"Disconnected from {portName}", "Disconnection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No connection to close.", "Disconnection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            RefreshCOMPorts();
        }

        private void TurnOnButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchControl.TurnOnSwitch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TurnOffButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switchControl.TurnOffSwitch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Frame1Input_KeyDown(object sender, KeyEventArgs e) => FrameInput_KeyDown(sender, e, 0);
        private void Frame2Input_KeyDown(object sender, KeyEventArgs e) => FrameInput_KeyDown(sender, e, 1);
        private void Frame3Input_KeyDown(object sender, KeyEventArgs e) => FrameInput_KeyDown(sender, e, 2);
        private void Frame4Input_KeyDown(object sender, KeyEventArgs e) => FrameInput_KeyDown(sender, e, 3);

        private void FrameInput_KeyDown(object sender, KeyEventArgs e, int frameIndex)
        {
            if (e.Key == Key.Enter)
            {
                TextBox? frameInputTextBox = sender as TextBox;
                string? textToSend = frameInputTextBox?.Text;
                if (!string.IsNullOrEmpty(textToSend))
                {
                    serialPortManager.SendData(frameIndex, textToSend);
                    frameInputTextBox?.Clear();
                    e.Handled = true;
                }
            }
        }

        private void ClearFrameData(int frameIndex)
        {
            switch (frameIndex)
            {
                case 0:
                    Frame1Data.Clear();
                    break;
                case 1:
                    Frame2Data.Clear();
                    break;
                case 2:
                    Frame3Data.Clear();
                    break;
                case 3:
                    Frame4Data.Clear();
                    break;
            }

            // Reload the latest traces after clearing
            RefreshFrameData(frameIndex);
        }

        private void RefreshFrameData(int frameIndex)
        {
            string? selectedPort = null;

            // Ensure that the frame index exists within the COMPortList items
            if (frameIndex < COMPortList.Items.Count)
            {
                selectedPort = COMPortList.Items[frameIndex]?.ToString();
            }

            // If selectedPort is still null or empty, and the port is connected, skip the error and proceed.
            if (string.IsNullOrEmpty(selectedPort))
            {
                if (_connectedPorts.ContainsKey(frameIndex))
                {
                    // Use the connected port
                    selectedPort = _connectedPorts[frameIndex];
                }
                else
                {
                    // If the port is not connected, show an error
                    MessageBox.Show($"No COM port available for frame {frameIndex + 1}.", "Port Not Available", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Exit the method if no valid port is available
                }
            }

            if (!string.IsNullOrEmpty(selectedPort))
            {
                int baudRate = 115200; // Use the desired baud rate
                Task.Run(async () =>
                {
                    bool connected = await serialPortManager.ConnectAsync(frameIndex, selectedPort, baudRate);
                    if (connected)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            serialPortManager.RegisterDataReceivedHandler(frameIndex, (s, args) =>
                            {
                                string data = serialPortManager.ReadExisting(frameIndex);
                                string dataType = serialPortManager.IdentifyDataType(data);
                                Dispatcher.Invoke(() => DisplayData(frameIndex, data, dataType, selectedPort));
                            });
                        });
                    }
                });
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex; // Get the currently selected frame
            ClearFrameData(selectedFrame); // Clear the data for the selected frame
        }

        private void COMPortList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void FrameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void BaudRateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        protected override void OnClosed(EventArgs e)
        {
            _toggleSwitchHandler?.DisposeResources();
            base.OnClosed(e);
        }

        private void Frame1Data_TextChanged(object sender, TextChangedEventArgs e) { }

        // Buttons Behavior

        // Method for Password Button Click
        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex;

            // Ensure a valid frame is selected
            if (selectedFrame >= 0 && selectedFrame < 4)
            {
                string command = "3CBPwd?";  // Command for Password

                // Send the command to the corresponding frame's serial port
                serialPortManager.SendData(selectedFrame, command);
            }
            else
            {
                MessageBox.Show("No frame selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method for Reset Button Click
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex;

            // Ensure a valid frame is selected
            if (selectedFrame >= 0 && selectedFrame < 4)
            {
                string command = "r";  // Command for Reset

                // Send the command to the corresponding frame's serial port
                serialPortManager.SendData(selectedFrame, command);
            }
            else
            {
                MessageBox.Show("No frame selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
