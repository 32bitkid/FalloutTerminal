using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FalloutTerminal.Communications;
using System.Linq;
using FalloutTerminal.RobcoIndustriesTermlink;

namespace FalloutTerminal.RobcoIndustriesTermlink.Apps
{
	class PasswordMemoryDump
	{
		public class ValidPassCode {
			public string Code { get; private set; }
			public int Position { get; private set; }
			public int Length { get; private set; }
			public bool Highlighted { get; set; }
			
			public ValidPassCode(string code, int position, int length) {
				Code = code;
				Position = position;
				Length = length;
				Highlighted = false;
			}
		}
		
		public delegate void WriteDelegate (string message);

		private static readonly char[] Junk = new[] { '!', '"', '\'', '$', '%', '#', '^', '&', '@', '=',
		'-', '^', '~', '|', '.', ',', ':', ';', '(', ']',
		'{', '>', '?', '/', '.', '.', '.', '.', '.', '.',
		'-', '-', '-', '=', '=', '=' };
		private readonly Random _rnd = new Random ();

		private readonly int _rows;
		private readonly int _cols;
		private readonly int _numberOfCols;
		private readonly int bytesPerColumn;

		public string CorrectPassword { get; private set; }
		private readonly char[] _memory;
		private readonly string[] _dict;
		private List<ValidPassCode> _passwords = new List<ValidPassCode>();

		public int BaseMemoryAddress { get; private set; }

		public PasswordMemoryDump (int rows, int cols, int numberOfCols, string[] dict)
		{
			BaseMemoryAddress = _rnd.Next (0xc000, 0xff00);
			_rows = rows;
			_cols = cols;
			_dict = dict;
			bytesPerColumn = _rows * _cols;
			_numberOfCols = numberOfCols;
			_memory = new char[rows * cols * numberOfCols];
			
			JunkFill ();
			CorrectPassword = _dict[_rnd.Next (_dict.Length)];
			FillTable ();
		}

		public void Write (WriteDelegate funct)
		{
			var sb = new StringBuilder ();
			
			for (var currentRow = 0; currentRow < _rows; currentRow++) {
				for (var currentColumn = 0; currentColumn < _numberOfCols; currentColumn++) {
					
					sb.AppendFormat ("0x{0:X4} ", currentRow * _cols + BaseMemoryAddress + bytesPerColumn * currentColumn);
					
					sb.Append (IBM3151.Commands.Intense);
					for (var columnIndex = 0; columnIndex < _cols; columnIndex++)
						sb.Append (this[currentRow * _cols + columnIndex + bytesPerColumn * currentColumn]);
					sb.Append (IBM3151.Commands.NotIntense);
					sb.Append ("   ");
				}
				
				if (currentRow != _rows - 1)
					sb.Append ("\r\n");
			}
			
			funct (sb.ToString ());
			
			
		}

		private void FillTable ()
		{
			var wordlist = new List<string> ();
			
			// Get a bunch that have something in common
			wordlist.AddRange (_dict.Where (delegate(string word) {
				if (word == CorrectPassword)
					return false;
				var n = CorrectPassword.Where ((t, i) => word[i] == t).Count ();
				return n >= 1 && n < 5;
			}).OrderBy (a => Guid.NewGuid ()).Take (10));
			
			
			// Get a few that have nothing in common
			wordlist.AddRange (_dict.Where (delegate(string word) {
				if (word == CorrectPassword)
					return false;
				var n = CorrectPassword.Where ((t, i) => word[i] == t).Count ();
				return n == 0;
			}).OrderBy (a => Guid.NewGuid ()).Take (4));
			
			// Get one thats really close
			wordlist.AddRange (_dict.Where (delegate(string word) {
				if (word == CorrectPassword)
					return false;
				var n = CorrectPassword.Where ((t, i) => word[i] == t).Count ();
				return n == 5;
			}).OrderBy (a => Guid.NewGuid ()).Take (1));
			
			// Add the correct password
			wordlist.Add (CorrectPassword);
			
			// Randomize the list (Yates shuffle)
			for(var i = wordlist.Count - 1; i > 0; i--) {
				var newIndex = _rnd.Next(0, i);
				var temp = wordlist[i];
				wordlist[i] = wordlist[newIndex];
				wordlist[newIndex] = temp;
			}
			
			var index = 0;
			var totalWords = wordlist.Count;
			var spacesLeft = _memory.Length;
			
			// Figure out how many spaces
			foreach(var word in wordlist)
				 spacesLeft -= word.Length + 1;
		
			// Set up skip table and distrubte the spaces evenly
			var skipTable = new int[totalWords + 1];
			for(var i = 0; i < totalWords + 1; i++)
				skipTable[i] = spacesLeft / (totalWords + 1);
			
			// Randomize skip table;
			for(var i = 0; i < 50; i++) {
				var a = _rnd.Next(totalWords + 1);
				var b = _rnd.Next(totalWords + 1);
				
				var range = Math.Min(skipTable[a], skipTable[b]);
				
				if(range < 1)
					continue;
				var delta = _rnd.Next(1,range / 2);
				
				skipTable[a] += delta;
				skipTable[b] -= delta;
			}
			
			for (var i = 0; i < totalWords; i++) {
				index += skipTable[i];
				_passwords.Add(new ValidPassCode(wordlist[i], index, wordlist[i].Length));
				index += CorrectPassword.Length + 1;
			}
			
			// Write the 
			foreach(var password in _passwords) 
				password.Code.CopyTo (0, _memory, password.Position, password.Length);
		}

		public int Check (string guess)
		{
			var paddedGuess = guess.PadRight (CorrectPassword.Length);
			return CorrectPassword.Where ((t, i) => paddedGuess[i] == t).Count ();
		}
		
		public void HighlightPossible(string guess, ISerialConnection terminal) {
			terminal.Write(IBM3151.Commands.SetCharacterAttribute(IBM3151.CharacterAttributes.Intense));
			
			foreach(var pw in _passwords.Where(x => x.Highlighted)) 
				UpdateCode(pw, false, terminal);
			
			if(guess != string.Empty) 
			{
				terminal.Write(IBM3151.Commands.SetCharacterAttribute(IBM3151.CharacterAttributes.Intense | IBM3151.CharacterAttributes.Reverse));
				foreach(var pw in _passwords.Where(x => x.Code.StartsWith(guess, StringComparison.CurrentCultureIgnoreCase))) 
					UpdateCode(pw, true, terminal);
			}
			
			terminal.Write(IBM3151.Commands.SetCharacterAttribute(IBM3151.CharacterAttributes.None));										
			terminal.Write(IBM3151.Commands.ResetBufferAddressMode);
			
		}
		
		private void UpdateCode(ValidPassCode pw, bool highlighted, ISerialConnection terminal) {
			for(var i = 0; i < pw.Code.Length; i++) {
				var pos = pw.Position + i;
				var row = (pos / _cols) % _rows + 6;
				var col = ((pos % _cols) + 8) + ((pos / bytesPerColumn) * 22);
				terminal.Write(IBM3151.Commands.SetBufferPosition(row, col));
				terminal.Write(pw.Code[i].ToString());
				pw.Highlighted = highlighted;
			}
		}

		private void JunkFill ()
		{
			for (var i = 0; i < _memory.Length; i++)
				_memory[i] = Junk[_rnd.Next (Junk.Length)];
		}

		public char this[int index] {
			get { return _memory[index]; }
		}
		
	}

	class DebugAccounts
	{
		private readonly IRobcoIndustriesTermlinkProtocol _termlink;

		private int _attempts;
		private PasswordMemoryDump _dump;
		const int MaxRows = 16;
		const int MaxCols = 12;
		const int NumCols = 2;

		public DebugAccounts (IRobcoIndustriesTermlinkProtocol termlink)
		{
			_termlink = termlink;
			_attempts = 4;
			_dump = new PasswordMemoryDump (MaxRows, MaxCols, NumCols, Dictionary.Words6);
			
		}

		private void UpdateAttempts ()
		{
			if (_attempts == 1) {
				_termlink.Connection.Write (IBM3151.Commands.SetBufferPosition (2, 1));
				_termlink.Connection.Write (IBM3151.Commands.SetCharacterAttribute (IBM3151.CharacterAttributes.Intense | IBM3151.CharacterAttributes.Blink));
				_termlink.Connection.Write (StaticMessages.HackingWarning);
				_termlink.Connection.Write (IBM3151.Commands.SetCharacterAttribute (IBM3151.CharacterAttributes.None));
			}
			_termlink.Connection.Write (IBM3151.Commands.SetBufferPosition (4, 1));
			_termlink.Connection.Write (string.Format ("{0} ATTEMPT(S) LEFT:", _attempts));
			_termlink.Connection.Write (IBM3151.Commands.Intense);
			
			for (var i = 0; i < _attempts; i++)
				if (_termlink.Connection is IBM3151.SerialTerminal)
					_termlink.Connection.Write (new byte[] { Ascii.SP, 254 }, 0, 2);
				else
					_termlink.Connection.Write (" *");
			
			
			_termlink.Connection.Write (new string (' ', (4 - _attempts) * 2));
			_termlink.Connection.Write (IBM3151.Commands.NotIntense);
		}

		public ReturnCodes Launch ()
		{
			_termlink.Connection.Write (IBM3151.Commands.ClearAll);
			_termlink.Connection.Write (IBM3151.Commands.Intense);
			_termlink.Connection.Write ("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL\r\n");
			_termlink.Connection.Write ("ENTER PASSWORD NOW\r\n\n");
			
			_termlink.Connection.Write (IBM3151.Commands.NotIntense);
			
			UpdateAttempts ();
			
			_termlink.Connection.Write ("\r\n\n");
			
			_dump.Write (_termlink.Connection.Write);
			return Prompt ();
		}

		private ReturnCodes Prompt ()
		{
			Console.WriteLine (_dump.CorrectPassword);
			while(true) {
				var pos = IBM3151.Commands.SetCursorPosition (21, 45);
				_termlink.Connection.Write (pos + (char)Ascii.BS + '>' + new string (' ', 80 - 45) + pos);
				
				var guess = _termlink.Connection.GetString(_dump.HighlightPossible);
				
				// If read sting is null then break.
				if (guess == null)
					return ReturnCodes.Abort;
				
				if (guess == _dump.CorrectPassword) {
					_termlink.SetAdminPassword (_dump.CorrectPassword);
					return ReturnCodes.Success;
				} else {
					var correct = _dump.Check(guess);
					Console.WriteLine (correct);
				}
				
				_attempts -= 1;
				
				if (_attempts == 0) {
					_termlink.Lockout ();
					return ReturnCodes.Failure;
				}
				
				UpdateAttempts ();
			}
			
		}
	}
}
