using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using FalloutTerminal.Communications;

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
			
			using(var s = new IBM3151.SerialTerminal("/dev/ttyS0", 38400, Parity.None, 8, StopBits.One))
			using(var v300 = new RobcoIndustriesTermlink.V300(s)) {
				
				s.Handshake = Handshake.XOnXOff;
				s.OperatingMode = IBM3151.OperatingModes.Echo;
				
				v300.Boot();
			}
			
		skip:
			
			Console.WriteLine ("Hello World!");
		}
	}
}
