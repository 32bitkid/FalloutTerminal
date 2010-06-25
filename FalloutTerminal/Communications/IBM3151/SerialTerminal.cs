using System;
using System.Text;
using System.IO.Ports;

namespace FalloutTerminal.Communications
{
	public partial class IBM3151 {
	
		public partial class SerialTerminal : SerialPort, ISerialConnection
		{
			public event EventHandler<EventArgs> Restart;			
			
			private byte[] _buffer = new byte[65536];
	
			public SerialTerminal(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
				:base(portName, baudRate, parity, dataBits, stopBits)
			{
				Open();
                Handshake = Handshake.XOnXOff;
                OperatingMode = IBM3151.OperatingModes.Echo;
			}
			
			public string GetString() {
				byte[] command = new byte[255], cleanBuffer = new byte[80];
	            int index = 0, cmdLen = 0;
	            var cursorPosition = 0;
	
	            do
	            {
	                //while(_serial.BytesToRead == 0) { Thread.Sleep(500); }
	                var readLength = Read(command, index, command.Length - index);
	                Write(command, index, readLength);
					
					//Console.WriteLine(BitConverter.ToString(command, index, readLength));
					
					var searchStart = 0;
					int escapeIndex;
					while((escapeIndex = Array.IndexOf<byte>(command, Ascii.ESC, searchStart)) != -1)
					{
						if(Restart != null && command[escapeIndex + 1] == Ascii.O)
						{
							Restart(this, EventArgs.Empty);
							return string.Empty;
						}
					}
					
					
					
	                index += readLength;
	
	            } while ((cmdLen = Array.IndexOf(command, Ascii.CR)) == -1);
	
	            var len = Unescape(command, 0, cmdLen, cleanBuffer);
	            return Encoding.ASCII.GetString(cleanBuffer, 0, len).ToUpper();
			}
			
	        private int Unescape(byte[] command, int start, int length, byte[] clean)
	        {
	            var cleanedLength = 0;
	
	            for(var i = start; i < length; i++)
	            {
	                if(command[i] >= 0x20 && command[i] <= 0x7e)
	                {
	                    clean[cleanedLength++] = command[i];
	                    continue;
	                }
	
	                if (command[i] == 0x08)
	                    cleanedLength--;
	            }
	
	            return cleanedLength;
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
