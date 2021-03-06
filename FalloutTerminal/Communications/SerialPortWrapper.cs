using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace FalloutTerminal.Communications
{
	
    class SerialPortWrapper : ISerialConnection
    {
		public event EventHandler<EventArgs> Restart;
		
        private readonly SerialPort _serialPort;

        public SerialPortWrapper(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }
		
		public int BytesToRead { get { return _serialPort.BytesToRead; } }


        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        public void Write(string text)
        {
            _serialPort.Write(text);
        }

        public void WriteLine(string text)
        {
            _serialPort.WriteLine(text);
        }

        public int ReadByte()
        {
            return _serialPort.ReadByte();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _serialPort.Read(buffer, offset, count);
        }
		
		public string GetString(bool masked) {
			throw new NotImplementedException();
		}
		
		public string GetString() {
			return GetString(false);
		}
		
		public string GetString(OnPressWorker worker) {
			return GetString(false);
		}
		

        public void Dispose() { }
    }
}
