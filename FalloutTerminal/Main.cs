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
			
			using(var s = new SerialTerminal("/dev/ttyS0", 2400, Parity.None, 8, StopBits.One)) {
				s.ClearAll();
				s.Write("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL\r\n");
				s.Write("ENTER PASSWORD NOW\r\n\n");
				s.Write("4 ATTEMPT(S) LEFT: \r\n\n");
				
				var sb = new StringBuilder();
				
				for(var i = 0; i < 16; i++) {
					sb.AppendFormat("0x{0:X4} ", i*12 + 0xF4F0);
					for(var j=0; j < 12; j++)
						sb.Append((char)rnd.Next(33,126));
					sb.Append("   ");
					sb.AppendFormat("0x{0:X4} ", i*12 + 0XF4F0 + 16 * 12);
					for(var j=0; j < 12; j++)
						sb.Append((char)rnd.Next(33,126));
					
					if(i != 15)
						sb.Append("\r\n");
				}
				sb.Append("  >");
				
				s.Write(sb.ToString());
				
				
				
			}
			
			Console.WriteLine ("Hello World!");
		}
	}
}
