using System;
using System.IO.Ports;
using System.Threading;
using System.Text;

namespace FalloutTerminal
{
	class MainClass
	{
		
		static ManualResetEvent mre = new ManualResetEvent(false);
		
		
		public static void Main (string[] args)
		{
			Console.CancelKeyPress += delegate {
				mre.Set();
			};
			
			Console.WriteLine ("Starting!");
			
			var rnd = new Random();
			
			using(var s = new SerialTerminal("/dev/ttyS0", 38400, Parity.None, 8, StopBits.One)) {
				s.OperatingMode = OperatingModes.Echo;
				
			start:
				s.Write(Commands.ClearAll);
				s.ReadByte();
				var sb = new StringBuilder();
				
				sb.Append(Commands.Intense);
				sb.Append("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL\r\n");
				sb.Append("ENTER PASSWORD NOW\r\n\n");
				
				sb.Append(Commands.NotIntense);
				sb.Append("4 ATTEMPT(S) LEFT: ");
				s.Write(sb.ToString());
				sb.Length = 0;
				
				s.Write(Commands.Intense);
				s.Write(new byte[] { 254, Ascii.SP, 254, Ascii.SP, 254, Ascii.SP, 254 }, 0, 7);
				s.Write(Commands.NotIntense);
				
				sb.Append("\r\n\n");
				
				
				for(var i = 0; i < 16; i++) {
					sb.AppendFormat("0x{0:X4}", i*12 + 0xF4F0);
					sb.Append(" ");
					
					sb.Append(Commands.Intense);
					for(var j=0; j < 12; j++)
						sb.Append((char)rnd.Next(33,126));
					sb.Append("   ");
					sb.Append(Commands.NotIntense);
					
					
					sb.AppendFormat("0x{0:X4} ", i*12 + 0XF4F0 + 16 * 12);

					sb.Append(Commands.Intense);
					for(var j=0; j < 12; j++)
						sb.Append((char)rnd.Next(33,126));
					sb.Append(Commands.NotIntense);
					
					if(i != 15)
						sb.Append("\r\n");
				}
				sb.Append("  >");
				
				s.Write(sb.ToString());
				
				byte b;
				while(true) {
					b = (byte)s.ReadByte();
					Console.WriteLine(":{0}", b);
					if(b == Ascii.CR)
						break;
					
					//6s.Write(new [] {b}, 0, 1);
				}
				
				s.Write(Commands.ClearAll);
				goto start;
				
			}
			
			Console.WriteLine ("Hello World!");
		}
	}
}
