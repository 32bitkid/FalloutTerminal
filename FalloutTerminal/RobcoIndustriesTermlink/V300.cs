using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FalloutTerminal.Communications;
using System.Threading;
using FalloutTerminal.RobcoIndustriesTermlink.Apps;

namespace FalloutTerminal.RobcoIndustriesTermlink
{
    public class V300 : IRobcoIndustriesTermlinkProtocol, IDisposable
    {
		public enum RunModes { Normal, Maint, DebugAccounts, MainApp }
		
		private readonly ISerialConnection _serial;

        public ISerialConnection Connection { get { return _serial; } }

		private readonly V300Parser _parser;
		
		private RunModes _runMode = RunModes.Normal;
		private bool _accountsProtected = true;
		private Random rnd = new Random();

        public V300(ISerialConnection serial)
        {
            _serial = serial;
			_parser = new V300Parser();
			_parser.HaltRestartMaint += HandleParserHaltRestartMaint;
			_parser.HaltRestartNormal += HandleParserHaltRestartNormal; 
			_parser.SetFileProtection += HandleParserSetFileProtection;
			_parser.RunDebugAccounts += HandleParserRunDebugAccounts;
        }

        void HandleParserHaltRestartNormal (object sender, ParserActionEventArgs e)
        {
			Boot(RunModes.Normal);
			e.Success = true;        	
        }

        void HandleParserRunDebugAccounts (object sender, ParserActionEventArgs e)
        {
       		if(_runMode != V300.RunModes.Maint || _accountsProtected) 
				return;
			
			e.Success = true;

            new DebugAccounts(this).Launch();
        }

        void HandleParserSetFileProtection (object sender, ParserActionEventArgs e)
        {
     		if(new Regex(@"ACCOUNTS.F$", RegexOptions.IgnoreCase).IsMatch(e.Options)) {
				e.Success = true;
				_accountsProtected = false;
			}
        }

        void HandleParserHaltRestartMaint (object sender, ParserActionEventArgs e)
        {
			Boot(RunModes.Maint);
			e.Success = true;
        }

        public void Boot(RunModes runMode)
        {
            _runMode = runMode;

			switch(runMode) {
			case RunModes.Maint:
				_serial.Write(StaticMessages.MaintainenceModeBootMessage);
				break;
            case RunModes.DebugAccounts:
                new DebugAccounts(this).Launch();
			    return;
			default:
	            _serial.Write(StaticMessages.NormalBootMessage);
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
		
		public void Dispose ()
        {
			
        }
    }
}
