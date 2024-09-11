using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Components.TracingWindow;
using WpfApp1;
using WpfApp1.Views.TracingWindow;

namespace MO_TERMINAL
{
    public partial class TracingWindow : Window
    {
        private SerialPortManager serialPortManager = new SerialPortManager();
        private ClewareSwitchControl switchControl;
        private ToggleSwitchHandler _toggleSwitchHandler;
        private bool isTraceFrozen = false; // Flag to check if trace is frozen
        private Dictionary<int, string> _connectedPorts = new Dictionary<int, string>(); // Track connected ports

        private bool[] isFrameAutoDetectionRunning = new bool[4] { true, true, true, true }; // Control auto-detection per frame
        private SerialPortFacade _serialPortFacade;


        public TracingWindow()
        {
            InitializeComponent();

            // Initialize the serial port facade
            _serialPortFacade = new SerialPortFacade();

            if (toggleSwitch != null)
            {
                _toggleSwitchHandler = new ToggleSwitchHandler(toggleSwitch);
                toggleSwitch.Checked += ToggleSwitch_Checked;
                toggleSwitch.Unchecked += ToggleSwitch_Unchecked;
            }
            else
            {
                MessageBox.Show("Toggle switch is not initialized.");
            }

            this.StateChanged += MainWindow_StateChanged;

            LoadCOMPorts();
            StartAutoDetection();  // Start auto-detection for available COM ports at startup
            switchControl = new ClewareSwitchControl();
        }


        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            _toggleSwitchHandler?.ToggleSwitch_Checked(sender, e);
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            _toggleSwitchHandler?.ToggleSwitch_Unchecked(sender, e);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            _toggleSwitchHandler?.MainWindow_StateChanged(sender, e);
        }

        private void LoadCOMPorts()
        {
            COMPortList.Items.Clear();
            string[] ports = serialPortManager.GetAvailablePorts();
            foreach (string port in ports)
            {
                COMPortList.Items.Add(port);
            }
        }

        private void RefreshCOMPorts()
        {
            LoadCOMPorts();
        }

        private void StartAutoDetection()
        {
            int maxFrames = 4; // Assuming 4 frames are used in your application

            for (int i = 0; i < maxFrames; i++)
            {
                if (i < COMPortList.Items.Count)
                {
                    string port = COMPortList.Items[i]?.ToString();
                    if (!string.IsNullOrEmpty(port))
                    {
                        int selectedFrame = i;
                        int baudRate = 115200; // Default baud rate for connection

                        Task.Run(async () =>
                        {
                            if (!isFrameAutoDetectionRunning[selectedFrame]) return;

                            bool connected = await serialPortManager.ConnectAsync(selectedFrame, port, baudRate);
                            if (connected)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    COMPortList.Items.Remove(port);
                                    _connectedPorts[selectedFrame] = port;  // Assign connected port to the frame
                                });

                                serialPortManager.RegisterDataReceivedHandler(selectedFrame, DataReceivedHandlerForFrame(selectedFrame));
                            }
                        });
                    }
                }
            }
        }


        private SerialDataReceivedEventHandler DataReceivedHandlerForFrame(int frameIndex)
        {
            return (s, args) =>
            {
                string data = serialPortManager.ReadExisting(frameIndex);
                string dataType = serialPortManager.IdentifyDataType(data);

                if (_connectedPorts.ContainsKey(frameIndex))
                {
                    Dispatcher.Invoke(() => DisplayData(frameIndex, data, dataType, _connectedPorts[frameIndex]));
                }
                else
                {
                    Dispatcher.Invoke(() => DisplayData(frameIndex, data, dataType, "Unknown Port"));
                }
            };
        }

        private void DisplayData(int frameIndex, string data, string dataType, string portName)
        {
            TextBox frameDataTextBox = null;
            ScrollViewer frameScrollViewer = null;
            TabItem frameTabItem = null;

            // Map the correct UI elements based on the frame index
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

            // Perform UI updates in a non-blocking manner
            if (frameDataTextBox != null)
            {
                Dispatcher.Invoke(() =>
                {
                    // Append data to the TextBox
                    frameDataTextBox.AppendText(data);

                    // Automatically scroll to the bottom of the frame's ScrollViewer if not frozen
                    if (!isTraceFrozen && frameScrollViewer != null)
                    {
                        ScrollToBottom(frameScrollViewer);
                    }

                    // Update the tab header based on the content in the data (WUC, NAD, V2X)
                    if (frameTabItem != null && !string.IsNullOrEmpty(data))
                    {
                        string currentHeader = frameTabItem.Header.ToString();

                        // Only update the header if it has changed to avoid flickering
                        if (data.Contains("W01") || data.Contains("WUC"))
                        {
                            string newHeader = $"WUC - {portName}";
                            if (!currentHeader.Equals(newHeader))
                            {
                                frameTabItem.Header = newHeader;
                            }
                        }
                        else if (data.Contains("ECALL_STATE") || data.Contains("NAD") || data.Contains("Enter HSM BL") || data.Contains("Set JTAG Done"))
                        {
                            string newHeader = $"NAD - {portName}";
                            if (!currentHeader.Equals(newHeader))
                            {
                                frameTabItem.Header = newHeader;
                            }
                        }
                        else if (data.Contains("V2X") || data.Contains("SEQ_A_OK."))
                        {
                            string newHeader = $"V2X - {portName}";
                            if (!currentHeader.Equals(newHeader))
                            {
                                frameTabItem.Header = newHeader;
                            }
                        }
                        else if (!currentHeader.Contains(portName)) // Only update if the port name is not already in the header
                        {
                            frameTabItem.Header = $"Frame {frameIndex + 1} - {portName}";
                        }
                    }
                });
            }
        }





        // Helper method to scroll to the bottom of the frame when new data arrives
        private void ScrollToBottom(ScrollViewer scrollViewer)
        {
            if (scrollViewer != null && !isTraceFrozen)
            {
                scrollViewer.ScrollToEnd(); // Trigger auto-scroll only when necessary
            }
        }


        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => Dispatcher.Invoke(() => LoadCOMPorts()));
            StartAutoDetection();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (COMPortList.SelectedItem != null && FrameSelector.SelectedIndex != -1 && BaudRateSelector.SelectedItem != null)
            {
                string selectedPort = COMPortList.SelectedItem?.ToString();
                int selectedFrame = FrameSelector.SelectedIndex;

                try
                {
                    // Validate selected port and baud rate
                    if (!string.IsNullOrEmpty(selectedPort) && int.TryParse((BaudRateSelector.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int baudRate))
                    {
                        isFrameAutoDetectionRunning[selectedFrame] = true;

                        // Attempt connection
                        bool connected = await Task.Run(() => serialPortManager.ConnectAsync(selectedFrame, selectedPort, baudRate));

                        if (connected)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                COMPortList.Items.Remove(selectedPort);
                                _connectedPorts[selectedFrame] = selectedPort;  // Ensure correct port is stored for this frame

                                // Update the tab header to show connected COM port
                                var frameTabItem = FrameTabControl.Items[selectedFrame] as TabItem;
                                if (frameTabItem != null)
                                {
                                    frameTabItem.Header = $"Frame {selectedFrame + 1} - {selectedPort}";
                                }
                            });

                            serialPortManager.RegisterDataReceivedHandler(selectedFrame, DataReceivedHandlerForFrame(selectedFrame));
                        }
                        else
                        {
                            MessageBox.Show("Error connecting to the selected port.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a valid COM port and baud rate.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a valid COM port and baud rate.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }





        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex;

            if (serialPortManager.IsConnected(selectedFrame))
            {
                isFrameAutoDetectionRunning[selectedFrame] = false;
                serialPortManager.UnregisterDataReceivedHandler(selectedFrame, DataReceivedHandlerForFrame(selectedFrame));
                serialPortManager.Disconnect(selectedFrame);
                ClearFrameData(selectedFrame);

                if (_connectedPorts.ContainsKey(selectedFrame))
                {
                    _connectedPorts.Remove(selectedFrame);
                }

                MessageBox.Show($"Successfully disconnected from Frame {selectedFrame + 1}.");
            }
            else
            {
                MessageBox.Show("No active connection to disconnect.");
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
                TextBox frameInputTextBox = sender as TextBox;
                string textToSend = frameInputTextBox?.Text;
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

            RefreshFrameData(frameIndex);
        }

        private void RefreshFrameData(int frameIndex)
        {
            string selectedPort = null;

            if (frameIndex < COMPortList.Items.Count)
            {
                selectedPort = COMPortList.Items[frameIndex]?.ToString();
            }

            if (string.IsNullOrEmpty(selectedPort) && _connectedPorts.ContainsKey(frameIndex))
            {
                selectedPort = _connectedPorts[frameIndex];
            }

            if (!string.IsNullOrEmpty(selectedPort))
            {
                int baudRate = 115200;

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

        private void FreezeTraceButton_Click(object sender, RoutedEventArgs e)
        {
            Button freezeButton = sender as Button;

            if (isTraceFrozen)
            {
                EnableAutoScroll();
                freezeButton.Content = "Freeze Trace";
            }
            else
            {
                DisableAutoScroll();
                freezeButton.Content = "Unfreeze Trace";
            }

            isTraceFrozen = !isTraceFrozen;
        }

        private void EnableAutoScroll()
        {
            ScrollToBottom(GetSelectedScrollViewer());
        }

        private void DisableAutoScroll() { }

        private ScrollViewer GetSelectedScrollViewer()
        {
            switch (FrameTabControl.SelectedIndex)
            {
                case 0: return Frame1ScrollViewer;
                case 1: return Frame2ScrollViewer;
                case 2: return Frame3ScrollViewer;
                case 3: return Frame4ScrollViewer;
                default: return null;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex;
            ClearFrameData(selectedFrame);
        }

        private void COMPortList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void FrameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void BaudRateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var port in _connectedPorts.Keys.ToList())
            {
                serialPortManager.Disconnect(port);
            }

            _toggleSwitchHandler?.DisposeResources();
            base.OnClosed(e);
        }

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

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedFrame = FrameTabControl.SelectedIndex;

            if (selectedFrame >= 0 && selectedFrame < 4)
            {
                // Check if the selected frame has a connected port
                if (_connectedPorts.ContainsKey(selectedFrame))
                {
                    string portName = _connectedPorts[selectedFrame];
                    string command = "3CBPwd?";  // Password command

                    // Ensure the port is connected before sending data
                    if (serialPortManager.IsConnected(selectedFrame))
                    {
                        // Simulate entering the password command into the respective frame's input field
                        TextBox frameInputTextBox = GetFrameInputTextBox(selectedFrame);
                        if (frameInputTextBox != null)
                        {
                            frameInputTextBox.Text = command;

                            // Simulate pressing 'Enter' to send the command
                            FrameInput_KeyDown(frameInputTextBox, new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(this), 0, Key.Enter)
                            {
                                RoutedEvent = Keyboard.KeyDownEvent
                            }, selectedFrame);

                            MessageBox.Show($"Password command sent to Frame {selectedFrame + 1} on {portName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Frame {selectedFrame + 1} is not connected to any port.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No valid port is connected to the selected frame.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No frame selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private TextBox GetFrameInputTextBox(int frameIndex)
        {
            switch (frameIndex)
            {
                case 0:
                    return Frame1Input;
                case 1:
                    return Frame2Input;
                case 2:
                    return Frame3Input;
                case 3:
                    return Frame4Input;
                default:
                    return null;
            }
        }






    }
}
