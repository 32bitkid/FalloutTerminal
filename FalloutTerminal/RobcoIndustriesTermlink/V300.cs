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
		
		private string _adminPassword = null;

        public V300(ISerialConnection serial)
        {
            _serial = serial;
			_parser = new V300Parser();
			_parser.HaltRestartMaint += HandleParserHaltRestartMaint;
			_parser.HaltRestartNormal += HandleParserHaltRestartNormal; 
			_parser.SetFileProtection += HandleParserSetFileProtection;
			_parser.RunDebugAccounts += HandleParserRunDebugAccounts;
			_parser.LogonAdmin += Handle_parserLogonAdmin;
			_serial.Restart += HandleSerialRestart;
        }
		
		public void SetAdminPassword(string password) {
			_adminPassword = password;
		}
		
		public void Lockout() {
			_serial.Write(new string('\n', 25));
			
			_serial.Write(IBM3151.Commands.Intense + StaticMessages.HackingLockout3);
			_serial.Write(StaticMessages.HackingLockout4 + IBM3151.Commands.NotIntense);
			_serial.Write(new string('\n', 10));
			
			Thread.Sleep(TimeSpan.FromSeconds(10));
		}

        void Handle_parserLogonAdmin (object sender, ParserActionEventArgs e)
        {
			for(var _attempts = 4; _attempts >= 0; _attempts--) {
     			_serial.Write("\r\n" + StaticMessages.HackingHeader2 + "\r\n\r\n");
				_serial.Write(IBM3151.Commands.Prompt);
				var guess = _serial.GetString(true);
				
				if(_adminPassword != null && guess == _adminPassword) 
				{
					return;
				}
				
				if(_attempts != 0) {
					_serial.Write("\n");
					_serial.Write(string.Format(StaticMessages.InvalidPassword, _attempts));
				}
			}
			
			Lockout();
			Boot(V300.RunModes.Normal);
			
			
        }

        void HandleSerialRestart (object sender, EventArgs e)
        {
			Boot(RunModes.Normal);		
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
			
			var command = _serial.GetString();
			_serial.Write("\n");
			
			var reply = _parser.Parse(command);
			
			if(reply == null) 
				return;
			
			_serial.Write(reply);
			Prompt();
		}
		
		public void Dispose ()
        {
			
        }
    }
}
