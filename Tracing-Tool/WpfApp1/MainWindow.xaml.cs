using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1;
using WpfApp1.Components; // Import the ToggleSwitchHandler class

namespace MO_TERMINAL
{
    public partial class MainWindow : Window
    {
        private SerialPortManager serialPortManager = new SerialPortManager();
        private ClewareSwitchControl switchControl;
        private ToggleSwitchHandler _toggleSwitchHandler; // Declare ToggleSwitchHandler

        public MainWindow()
        {
            InitializeComponent(); // Initialize the components first

            // Check if toggleSwitch is available and then initialize the handler
            if (toggleSwitch != null)
            {
                _toggleSwitchHandler = new ToggleSwitchHandler(toggleSwitch);

                // Now link the ToggleButton's Checked and Unchecked events after handler initialization
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
            COMPortList.Items.Clear();
            string[] ports = serialPortManager.GetAvailablePorts();
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
                frameDataTextBox.AppendText(data);
                ScrollToBottom(frameScrollViewer!);

                if (!string.IsNullOrEmpty(dataType) && frameTabItem != null)
                {
                    frameTabItem.Header = $"{dataType} - {portName}";
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
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (COMPortList.SelectedItem != null && FrameSelector.SelectedIndex != -1 && BaudRateSelector.SelectedItem != null)
            {
                string? selectedPort = COMPortList.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedPort) && int.TryParse((BaudRateSelector.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int baudRate))
                {
                    int selectedFrame = FrameSelector.SelectedIndex;

                    bool connected = await serialPortManager.ConnectAsync(selectedFrame, selectedPort, baudRate);

                    if (connected)
                    {
                        Dispatcher.InvokeAsync(() => COMPortList.Items.Remove(selectedPort));

                        serialPortManager.RegisterDataReceivedHandler(selectedFrame, (s, args) =>
                        {
                            var task = Task.Run(() =>
                            {
                                string data = serialPortManager.ReadExisting(selectedFrame);
                                string dataType = serialPortManager.IdentifyDataType(data);
                                Dispatcher.InvokeAsync(() => DisplayData(selectedFrame, data, dataType, selectedPort));
                            });
                        });

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

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem? selectedTab = FrameTabControl.SelectedItem as TabItem;
            int selectedFrame = FrameTabControl.SelectedIndex;

            if (serialPortManager.IsConnected(selectedFrame))
            {
                serialPortManager.Disconnect(selectedFrame);
                ClearFrameData(selectedFrame);

                if (selectedTab != null)
                {
                    string portName = selectedTab.Header.ToString().Split('-').Last().Trim();
                    Dispatcher.InvokeAsync(() => COMPortList.Items.Add(portName));
                    selectedTab.Header = $"Frame {selectedFrame + 1}";
                    MessageBox.Show($"Disconnected from {portName}", "Disconnection Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No connection to close.", "Disconnection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
        }

        private void COMPortList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void FrameSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void BaudRateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        protected override void OnClosed(EventArgs e)
        {
            _toggleSwitchHandler?.DisposeResources();
            base.OnClosed(e);
        }
    }
}
