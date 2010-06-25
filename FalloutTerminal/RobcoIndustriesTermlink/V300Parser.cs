
using System;
using System.Text;
using System.Text.RegularExpressions;
using FalloutTerminal.Communications;
namespace FalloutTerminal.RobcoIndustriesTermlink
{
	public class ParserActionEventArgs : EventArgs {
		public bool Success { get; set; }
		public string Options { get; set; }
	}
	
	public class V300Parser
	{
		public event EventHandler<ParserActionEventArgs> HaltRestartMaint;
		public event EventHandler<ParserActionEventArgs> HaltRestartNormal;
		public event EventHandler<ParserActionEventArgs> SetFileProtection;
		public event EventHandler<ParserActionEventArgs> RunDebugAccounts;
		public event EventHandler<ParserActionEventArgs> LogonAdmin;
		
		private static Regex SetCommand = new Regex(@"^SET\s+(\w[\w ]*/\w+)(=(.*))?$", RegexOptions.IgnoreCase);
		private static Regex RunCommand = new Regex(@"^RUN\s+(\w*)/?(.+)?$", RegexOptions.IgnoreCase);
		private static Regex LogonCommand = new Regex(@"^LOGON\s+(\w*)?$", RegexOptions.IgnoreCase);
		
		public V300Parser() { }
		
		public string Parse(string command) {
			if(SetCommand.IsMatch(command)) 
				return ParseSet(command);
			
			if(RunCommand.IsMatch(command)) 
				return ParseRun(command);
			
			if(LogonCommand.IsMatch(command))
				return ParseLogon(command);
			
			if(command.StartsWith("DIR", StringComparison.CurrentCultureIgnoreCase))
				return StaticMessages.LoginRequired;
			
			return StaticMessages.BadCommand;
		}
		
		private string ParseLogon(string command) {
			var match = LogonCommand.Match(command);
			var user = match.Groups[1].ToString().ToUpper();
			
			if(user == "ADMIN" && LogonAdmin != null) {
				LogonAdmin(this, new ParserActionEventArgs());
				return null;
			}
			
			return StaticMessages.BadUser;
		}
		
		private string ParseRun(string command) {
			var match = RunCommand.Match(command);
			var mode = match.Groups[1].ToString().ToUpper();
			var file = match.Groups[2].ToString().ToUpper();
			
			//Console.WriteLine(file);
			//Console.WriteLine(mode);
			
			if(mode == "DEBUG" && file == "ACCOUNTS.F") {
				if(RunDebugAccounts != null) {
					var e = new ParserActionEventArgs();
					RunDebugAccounts(this, e);
					if(e.Success)
					{
						if(HaltRestartNormal != null) HaltRestartNormal(this, new ParserActionEventArgs());
						return null;
					}
				}
				
				return StaticMessages.AccessDenied;
			}
			
				
			return StaticMessages.BadCommand;
		}
		
		private string ParseSet(string command) {
			var match = SetCommand.Match(command);
			var cmd = match.Groups[1].ToString().ToUpper();
			var options = match.Groups[3].ToString();
			
			ParserActionEventArgs e;
			
			switch(cmd) {
			case "TERMINAL/INQUIRE":
				return StaticMessages.HackingIntro03;
			case "FILE/PROTECTION":
				e = new ParserActionEventArgs() { Options = options };
				if(SetFileProtection != null) SetFileProtection(this, e);
				return e.Success ? string.Empty : StaticMessages.BadCommand;
			case "HALT RESTART/NORMAL":
				e = new ParserActionEventArgs() { Options = options };
				if(HaltRestartNormal != null) HaltRestartNormal(this, e);
				return e.Success ? null : StaticMessages.AccessDenied;
			case "HALT RESTART/MAINT":
				e = new ParserActionEventArgs() { Options = options };
				if(HaltRestartMaint != null) HaltRestartMaint(this, e);
				return e.Success ? null : StaticMessages.AccessDenied;
			}
			
			return StaticMessages.BadCommand;				
		}
	}
}
