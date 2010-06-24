using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FalloutTerminal.Communications;
using System.Threading;

namespace FalloutTerminal.RobcoIndustriesTermlink
{
    public class V300 : IRobcoIndustriesTermlinkProtocol, IDisposable
    {
		public enum RunModes { Normal, Maint, DebugAccounts, MainApp }
		
		private readonly ISerialConnection _serial;
		private readonly V300Parser _parser;
		
		private RunModes _runMode = RunModes.Normal;
		private bool _accountsProtected = true;
		private Random rnd = new Random();

        public V300(ISerialConnection serial)
        {
            _serial = serial;
			_parser = new V300Parser();
			_parser.HaltRestartMaint += Handle_parserHaltRestartMaint;
			_parser.HaltRestartNormal += Handle_parserHaltRestartNormal; 
			_parser.SetFileProtection += Handle_parserSetFileProtection;
			_parser.RunDebugAccounts += Handle_parserRunDebugAccounts;
        }

        void Handle_parserHaltRestartNormal (object sender, ParserActionEventArgs e)
        {
			_runMode = RunModes.Normal;
			Boot();
			e.Success = true;        	
        }

        void Handle_parserRunDebugAccounts (object sender, ParserActionEventArgs e)
        {
       		if(_runMode != V300.RunModes.Maint || _accountsProtected) 
				return;
			
			e.Success = true;
			ShowDebugAccounts();
        }

        void Handle_parserSetFileProtection (object sender, ParserActionEventArgs e)
        {
     		if(new Regex(@"ACCOUNTS.F$", RegexOptions.IgnoreCase).IsMatch(e.Options)) {
				e.Success = true;
				_accountsProtected = false;
			}
        }

        void Handle_parserHaltRestartMaint (object sender, ParserActionEventArgs e)
        {
			_runMode = RunModes.Maint;
			Boot();
			e.Success = true;
        }

        public void Boot()
        {
			switch(_runMode) {
			case RunModes.Maint:
				_serial.Write(Strings.MaintainenceModeBootMessage);
				break;
			default:
				_runMode = RunModes.Normal;
				_serial.Write(new byte[] { Ascii.Bell }, 0, 1);
				_serial.Write(IBM3151.Commands.ClearAll);
	            _serial.Write(Strings.NormalBootMessage);
				break;				
			}
			Prompt();
        }
		
		public void Prompt() {
			_serial.Write(IBM3151.Commands.Prompt);
			
			var command = new byte[255];
			int index = 0, cmdLen = 0;
			var readBuffer = new byte[80];
			
			do {
				//while(_serial.BytesToRead == 0) { Thread.Sleep(500); }
				var readLength = _serial.Read(command, index, command.Length - index);
				_serial.Write(command, index, readLength);
				index += readLength;
			} while((cmdLen = Array.IndexOf(command, Ascii.CR)) == -1);
			
			_serial.Write("\n");
			
			var reply = _parser.Parse(command, cmdLen);
			if(reply != null) {
				_serial.Write(reply);
				Prompt();
			}
		}
		
		public void ShowDebugAccounts() {
			var sb = new StringBuilder();
			
			sb.Append(IBM3151.Commands.ClearAll);
			sb.Append(IBM3151.Commands.Intense);
			sb.Append("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL\r\n");
			sb.Append("ENTER PASSWORD NOW\r\n\n");
			
			sb.Append(IBM3151.Commands.NotIntense);
			sb.Append("4 ATTEMPT(S) LEFT: ");
			_serial.Write(sb.ToString());
			sb.Length = 0;
			
			_serial.Write(IBM3151.Commands.Intense);
			_serial.Write(new byte[] { 254, Ascii.SP, 254, Ascii.SP, 254, Ascii.SP, 254 }, 0, 7);
			_serial.Write(IBM3151.Commands.NotIntense);
			
			sb.Append("\r\n\n");
			
			
			for(var i = 0; i < 16; i++) {
				sb.AppendFormat("0x{0:X4}", i*12 + 0xF4F0);
				sb.Append(" ");
				
				sb.Append(IBM3151.Commands.Intense);
				for(var j=0; j < 12; j++)
					sb.Append((char)rnd.Next(33,126));
				sb.Append("   ");
				sb.Append(IBM3151.Commands.NotIntense);
				
				
				sb.AppendFormat("0x{0:X4} ", i*12 + 0XF4F0 + 16 * 12);
	
				sb.Append(IBM3151.Commands.Intense);
				for(var j=0; j < 12; j++)
					sb.Append((char)rnd.Next(33,126));
				sb.Append(IBM3151.Commands.NotIntense);
				
				if(i != 15)
					sb.Append("\r\n");
			}
			sb.Append("  >");
			
			_serial.Write(sb.ToString());
		}
		
		public void Dispose ()
        {
			
        }
    }
}
