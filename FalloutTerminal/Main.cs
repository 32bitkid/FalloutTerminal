using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using FalloutTerminal.Communications;
using FalloutTerminal.RobcoIndustriesTermlink;

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
			
			using(var s = new IBM3151.SerialTerminal("/dev/ttyS0", 19200, Parity.None, 8, StopBits.One))
            //using(var s = new Fake3151Console())
			using(var v300 = new V300(s)) {
			    v300.Boot(V300.RunModes.DebugAccounts);
			}
			
		skip:
			
			Console.WriteLine ("Hello World!");
		}
	}
}
