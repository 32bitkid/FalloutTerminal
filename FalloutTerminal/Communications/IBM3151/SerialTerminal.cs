using System;
using System.IO.Ports;

namespace FalloutTerminal.Communications
{
	public partial class IBM3151 {
	
		public partial class SerialTerminal : SerialPort, ISerialConnection
		{
			private byte[] _buffer = new byte[65536];
	
			public SerialTerminal(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
				:base(portName, baudRate, parity, dataBits, stopBits)
			{
				Open();
                Handshake = Handshake.XOnXOff;
                OperatingMode = IBM3151.OperatingModes.Echo;
			}
			
			private int ReadResponse(byte[] buffer, int index) {
				// Look for LTA
				do { 
					buffer[index] = (byte)this.ReadByte();
				} while (buffer[index] != Ascii.CR && 
					     buffer[index] != Ascii.ETX &&
					     buffer[index] != Ascii.EOT &&
					     buffer[index] != Ascii.DC3 &&
				         (++index != buffer.Length)); // Buffer overflow
				
				return index;
			}
		}
	}
}
