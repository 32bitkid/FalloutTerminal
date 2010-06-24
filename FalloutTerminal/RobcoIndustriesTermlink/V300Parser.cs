
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
		
		private static Regex SetCommand = new Regex(@"^SET\s+(\w[\w ]*/\w+)(=(.*))?$", RegexOptions.IgnoreCase);
		private static Regex RunCommand = new Regex(@"^RUN\s+(\w*)/?(.+)?$", RegexOptions.IgnoreCase);
		
		public V300Parser() { }
		
		private string Clean(byte[] buffer, int length) {
			int startIndex = 0, pos, min;
			
			while(buffer[startIndex] == Ascii.BS)
				++startIndex;
			
			while((pos = Array.IndexOf(buffer, Ascii.BS)) != -1) {
				
				min = pos == 0 ? 0 : pos - 1;
				Array.Copy(buffer, pos + 1, buffer, min, length);
				length -= pos == 0 ? 1 : 2;
			}
			
			return Encoding.ASCII.GetString(buffer, startIndex, length).Trim();
		}
		
		public string Parse(byte[] buffer, int length) {
			var command = Clean(buffer, length);
			//Console.WriteLine(command);
			
			if(SetCommand.IsMatch(command)) 
				return ParseSet(command);
			
			if(RunCommand.IsMatch(command)) 
				return ParseRun(command);
			
			return StaticMessages.BadCommand;
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
						return null;
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
