using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using FalloutTerminal.Communications;
using System.Linq;

namespace FalloutTerminal.RobcoIndustriesTermlink.Apps
{
    class PasswordMemoryDump
    {
        private static readonly char[]
            Junk = new[] {
                '!', '"', '\'', '$', '%', '#', '^', '&',
                '@', '=', '-', '^', '~', '|', '.', ',',
                ':', ';', '(', ']', '{', '>', '?', '/',
                '.', '.', '.', '.', '.', '.', 
                '-', '-', '-', '=', '=', '=', 
            };
        private readonly Random _rnd = new Random();

        private readonly int _rows;
        private readonly int _cols;
        public string CorrectPassword { get; private set; }
        private readonly char[] _memory;
        private readonly string[] _dict;

        public int BaseMemoryAddress { get; private set; }

        public PasswordMemoryDump(int rows, int cols, string[] dict)
        {
            BaseMemoryAddress = _rnd.Next(0xC000, 0xFF00);
            _rows = rows;
            _cols = cols;
            _dict = dict;
            _memory = new char[rows * cols];

            JunkFill();
            CorrectPassword = _dict[_rnd.Next(_dict.Length)];
            FillTable();
        }

        private void FillTable()
        {
            var others = _dict.Where(delegate(string word)
                             {
                                 if (word == CorrectPassword)
                                     return false;
                                 var n = CorrectPassword.Where((t, i) => word[i] == t).Count();
                                 return n > 1;
                             }).OrderBy(a => Guid.NewGuid()).Take(_rows).ToList();


            others.Add(CorrectPassword);
            others.OrderBy(a => Guid.NewGuid());

            var index = 0;
            var max = others.Count;
            var spacesLeft = _memory.Length - (max * (CorrectPassword.Length + 1));
            
            for(var i = 0; i< max; i++)
            {
                var skip = _rnd.Next(3, spacesLeft / (max - i));
                spacesLeft -= skip;
                index += skip;

                others[i].CopyTo(0, _memory, index, CorrectPassword.Length);
                index += CorrectPassword.Length + 1;
            }
            
            
        }

        public int Check(string guess)
        {
            var paddedGuess = guess.PadRight(CorrectPassword.Length);
            return CorrectPassword.Where((t, i) => paddedGuess[i] == t).Count();
        }

        private void JunkFill()
        {
            for(var i = 0; i < _rows * _cols; i++)
                _memory[i] = Junk[_rnd.Next(Junk.Length)];
        }

        public char this [int index]
        {
            get
            {
                return _memory[index];
            }
        }

    }

    class DebugAccounts
    {
        private readonly IRobcoIndustriesTermlinkProtocol _termlink;
        
        private int _attempts;
        private PasswordMemoryDump _dump;
        const int MaxRows = 16;
        const int MaxCols = 12;

        public DebugAccounts(IRobcoIndustriesTermlinkProtocol termlink)
        {
            _termlink = termlink;
            _attempts = 4;
            _dump = new PasswordMemoryDump(MaxRows, MaxCols * 2, Dictionary.Words6);

        }

        private void UpdateAttempts()
        {
            if(_attempts == 1)
            {
                _termlink.Connection.Write(IBM3151.Commands.SetCursorPosition(2, 1));
                _termlink.Connection.Write(IBM3151.Commands.SetCharacterAttribute(IBM3151.CharacterAttributes.Intense | IBM3151.CharacterAttributes.Blink));
                _termlink.Connection.Write(StaticMessages.HackingWarning);
                _termlink.Connection.Write(IBM3151.Commands.SetCharacterAttribute(IBM3151.CharacterAttributes.None));
            }
            _termlink.Connection.Write(IBM3151.Commands.SetCursorPosition(4,1));
            _termlink.Connection.Write(string.Format("{0} ATTEMPT(S) LEFT:", _attempts));
            _termlink.Connection.Write(IBM3151.Commands.Intense);

            for (var i = 0; i < _attempts; i++)
                if(_termlink is IBM3151.SerialTerminal)
                    _termlink.Connection.Write(new byte[] { Ascii.SP , 254}, 0, 2);
                else
                    _termlink.Connection.Write(" *");
                    

            _termlink.Connection.Write(new string(' ', (4 - _attempts)*2));
            _termlink.Connection.Write(IBM3151.Commands.NotIntense);
        }

        private void Success()
        {
            _termlink.Boot(V300.RunModes.Normal);
        }

        public void Launch()
        {
            var sb = new StringBuilder();

            sb.Append(IBM3151.Commands.ClearAll);
            sb.Append(IBM3151.Commands.Intense);
            sb.Append("ROBCO INDUSTRIES (TM) TERMLINK PROTOCOL\r\n");
            sb.Append("ENTER PASSWORD NOW\r\n\n");

            sb.Append(IBM3151.Commands.NotIntense);

            _termlink.Connection.Write(sb.ToString());
            sb.Length = 0;

            UpdateAttempts();

            sb.Append("\r\n\n");

            for (var i = 0; i < MaxRows; i++)
            {
                sb.AppendFormat("0x{0:X4} ", i * MaxCols + _dump.BaseMemoryAddress);

                sb.Append(IBM3151.Commands.Intense);
                for (var j = 0; j < MaxCols; j++)
                    sb.Append(_dump[i * MaxCols + j]);
                sb.Append("   ");
                sb.Append(IBM3151.Commands.NotIntense);


                sb.AppendFormat("0x{0:X4} ", i * MaxCols + _dump.BaseMemoryAddress + MaxRows * MaxCols);

                sb.Append(IBM3151.Commands.Intense);
                for (var j = 0; j < MaxCols; j++)
                    sb.Append(_dump[i * MaxCols + j + MaxRows * MaxCols]);
                sb.Append(IBM3151.Commands.NotIntense);

                if (i != MaxRows - 1)
                    sb.Append("\r\n");
            }

            _termlink.Connection.Write(sb.ToString());
            Prompt();
        }

        private void Prompt()
        {
            var pos = IBM3151.Commands.SetCursorPosition(21, 45);
            _termlink.Connection.Write(pos + (char) Ascii.BS + '>' + new string(' ', 80 - 45) + pos);

			var guess = _termlink.Connection.GetString();

            if (guess == _dump.CorrectPassword)
            {
                return;
            }
            else
            {
                var correct = _dump.Check(guess);
				Console.WriteLine(correct);
            }

            _attempts -= 1;
            UpdateAttempts();
            

            Prompt();
        }
    }
}
