using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public class SerialPortManager : IDisposable
    {
        private SerialPort[] serialPorts = new SerialPort[4];
        private readonly Dictionary<int, string> _connectedPorts = new Dictionary<int, string>();
        private readonly object _lock = new object();  // To ensure thread safety

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public async Task<bool> ConnectAsync(int frameIndex, string portName, int baudRate)
        {
            // Avoid re-opening an already connected port
            if (serialPorts[frameIndex] == null && !string.IsNullOrEmpty(portName))
            {
                return await Task.Run(() =>
                {
                    lock (_lock) // Ensure thread safety when accessing serial ports
                    {
                        try
                        {
                            serialPorts[frameIndex] = new SerialPort(portName, baudRate)
                            {
                                DtrEnable = true, // Ensure proper serial communication setup
                                RtsEnable = true
                            };
                            serialPorts[frameIndex].Open();
                            _connectedPorts[frameIndex] = portName;
                            return true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            ReportError($"Port {portName} is already in use or access is denied.", "Connection Error");
                        }
                        catch (IOException)
                        {
                            ReportError($"Port {portName} is not available or does not exist.", "Connection Error");
                        }
                        catch (Exception ex)
                        {
                            ReportError($"Error connecting to port {portName}: {ex.Message}", "Connection Error");
                        }
                        finally
                        {
                            if (serialPorts[frameIndex] == null || !serialPorts[frameIndex].IsOpen)
                            {
                                serialPorts[frameIndex]?.Dispose();
                                serialPorts[frameIndex] = null;
                            }
                        }
                        return false;
                    }
                });
            }
            return false;
        }

        public void Disconnect(int frameIndex)
        {
            lock (_lock)
            {
                if (serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen)
                {
                    try
                    {
                        serialPorts[frameIndex].Close();
                    }
                    catch (IOException ex)
                    {
                        ReportError($"An error occurred while disconnecting: {ex.Message}", "Disconnection Error");
                    }
                    finally
                    {
                        serialPorts[frameIndex]?.Dispose();
                        serialPorts[frameIndex] = null;
                        _connectedPorts.Remove(frameIndex);
                    }
                }
            }
        }

        public bool IsConnected(int frameIndex)
        {
            lock (_lock)
            {
                return serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen;
            }
        }

        public void SendData(int frameIndex, string data)
        {
            if (IsConnected(frameIndex))
            {
                Task.Run(() =>
                {
                    try
                    {
                        lock (_lock)
                        {
                            serialPorts[frameIndex]?.WriteLine(data);  // Ensure thread safety for sending data
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportError($"Error sending data to port: {ex.Message}", "Send Data Error");
                    }
                });
            }
        }

        public void RegisterDataReceivedHandler(int frameIndex, SerialDataReceivedEventHandler handler)
        {
            lock (_lock)
            {
                if (serialPorts[frameIndex] != null)
                {
                    serialPorts[frameIndex].DataReceived += handler;
                }
            }
        }

        public void UnregisterDataReceivedHandler(int frameIndex, SerialDataReceivedEventHandler handler)
        {
            lock (_lock)
            {
                if (serialPorts[frameIndex] != null)
                {
                    serialPorts[frameIndex].DataReceived -= handler;
                }
            }
        }

        public string ReadExisting(int frameIndex)
        {
            lock (_lock)
            {
                if (serialPorts[frameIndex] != null && serialPorts[frameIndex].IsOpen)
                {
                    try
                    {
                        return serialPorts[frameIndex].ReadExisting();
                    }
                    catch (IOException ex)
                    {
                        ReportError($"Error reading data from port: {ex.Message}", "Read Data Error");
                    }
                }
            }
            return string.Empty;
        }

        public string IdentifyDataType(string data)
        {
            // Simple pattern matching logic to identify data types
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

        private void ReportError(string message, string title)
        {
            // Move this to a background thread to avoid blocking the UI
            Task.Run(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void Dispose()
        {
            // Clean up any open ports when disposing
            for (int i = 0; i < serialPorts.Length; i++)
            {
                Disconnect(i);  // This will handle closing and disposing the ports
            }
        }
    }
}
