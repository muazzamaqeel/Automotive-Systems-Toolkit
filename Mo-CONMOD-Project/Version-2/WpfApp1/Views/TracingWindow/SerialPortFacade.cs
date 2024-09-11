using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Views.TracingWindow
{
    internal class SerialPortFacade
    {
        private readonly SerialPortManager _serialPortManager;
        private Dictionary<int, string> _connectedPorts = new Dictionary<int, string>();

        public SerialPortFacade()
        {
            _serialPortManager = new SerialPortManager();
        }

        public string[] GetAvailablePorts()
        {
            return _serialPortManager.GetAvailablePorts();
        }

        public async Task<bool> ConnectToPort(int frameIndex, string port, int baudRate)
        {
            if (await _serialPortManager.ConnectAsync(frameIndex, port, baudRate))
            {
                _connectedPorts[frameIndex] = port;
                return true;
            }
            return false;
        }

        public void DisconnectPort(int frameIndex)
        {
            if (_serialPortManager.IsConnected(frameIndex))
            {
                _serialPortManager.Disconnect(frameIndex);
                _connectedPorts.Remove(frameIndex);
            }
        }

        public bool IsPortConnected(int frameIndex)
        {
            return _serialPortManager.IsConnected(frameIndex);
        }

        public void RegisterDataHandler(int frameIndex, SerialDataReceivedEventHandler handler)
        {
            _serialPortManager.RegisterDataReceivedHandler(frameIndex, handler);
        }

        public void UnregisterDataHandler(int frameIndex)
        {
            _serialPortManager.UnregisterDataReceivedHandler(frameIndex, null);
        }

        public string ReadData(int frameIndex)
        {
            return _serialPortManager.ReadExisting(frameIndex);
        }

        public void SendData(int frameIndex, string data)
        {
            _serialPortManager.SendData(frameIndex, data);
        }
    }



}
