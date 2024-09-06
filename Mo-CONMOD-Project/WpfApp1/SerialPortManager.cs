using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class SerialPortManager
    {
        private SerialPort[] serialPorts = new SerialPort[4];

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public async Task<bool> ConnectAsync(int frameIndex, string portName, int baudRate)
        {
            if (serialPorts[frameIndex] == null && !string.IsNullOrEmpty(portName))
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        serialPorts[frameIndex] = new SerialPort(portName, baudRate);
                        serialPorts[frameIndex].Open();
                        return true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show($"Port {portName} is already in use or access is denied.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        serialPorts[frameIndex] = null;
                        return false;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show($"Port {portName} is not available or does not exist.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        serialPorts[frameIndex] = null;
                        return false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while connecting to port {portName}: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        serialPorts[frameIndex] = null;
                        return false;
                    }
                });
            }
            return false;
        }

        public void Disconnect(int frameIndex)
        {
            if (serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen)
            {
                try
                {
                    serialPorts[frameIndex].Close();
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"An error occurred while disconnecting: {ex.Message}", "Disconnection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    serialPorts[frameIndex].Dispose();
                    serialPorts[frameIndex] = null;
                }
            }
        }

        public bool IsConnected(int frameIndex)
        {
            return serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen;
        }

        public void SendData(int frameIndex, string data)
        {
            if (IsConnected(frameIndex))
            {
                serialPorts[frameIndex].WriteLine(data);
            }
        }

        public void RegisterDataReceivedHandler(int frameIndex, SerialDataReceivedEventHandler handler)
        {
            if (serialPorts[frameIndex] != null)
            {
                serialPorts[frameIndex].DataReceived += handler;
            }
        }

        public void UnregisterDataReceivedHandler(int frameIndex, SerialDataReceivedEventHandler handler)
        {
            if (serialPorts[frameIndex] != null)
            {
                serialPorts[frameIndex].DataReceived -= handler;
            }
        }

        public string ReadExisting(int frameIndex)
        {
            if (serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen)
            {
                return serialPorts[frameIndex].ReadExisting();
            }
            return string.Empty;
        }

        public string IdentifyDataType(string data)
        {
            // Simple pattern matching logic to identify WUC and NAD
            if (data.Contains("stf") && data.Contains("KL30"))
            {
                return "WUC";
            }
            else if (data.Contains("PowerStateDispatcher"))
            {
                return "NAD";
            }
            return string.Empty;
        }
    }
}
