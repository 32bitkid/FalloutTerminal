using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FalloutTerminal.Communications
{
    public class Fake3151Console : ISerialConnection
    {

        public Fake3151Console()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
        public void Write(byte[] buffer, int offset, int count)
        {
            Write(Encoding.Default.GetString(buffer, offset, count));
        }

        public void Write(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                switch(text[i])
                {
                    case (char)Ascii.ESC:
                        i += ParseEscapeCode(text, i);
                       break;
                    default:
                        Thread.Sleep(1);
                        Console.Write(text[i]);
                        break;
                }
            }
        }

        private static int ParseEscapeCode(string text, int i)
        {
            if(text[i + 1] == '!' && text[i+2] == 'L')
            {
                Console.Clear();
                return 2;
            }
            if(text[i + 1] == 'I')
            {
                Console.CursorLeft = 0;
                Console.WriteLine(new string(' ', Console.BufferWidth - 1));
                Console.CursorLeft = 0;
                Console.CursorTop -= 1;
                return 1;
            }

            if(text[i+1] == 'Y')
            {
                var row = (byte)(text[i + 2] - 0x21) ;
                var col = (byte)(text[i + 3] - 0x21);
                Console.CursorLeft = col;
                Console.CursorTop = row;

                return 3;
            }
            
            if(text[i+1] == '4')
            {
                var code = (IBM3151.CharacterAttributes) text[i + 2];
                var hasOp = (code & IBM3151.CharacterAttributes.Op) ==
                            IBM3151.CharacterAttributes.Op;

                var op = hasOp ? (IBM3151.OperationSpecifier)text[i + 3] : 0;
                if(!hasOp || (op == IBM3151.OperationSpecifier.Replacement))
                {
                    if ((code & IBM3151.CharacterAttributes.Intense) == IBM3151.CharacterAttributes.Intense)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkGreen;

                    return 2;
                }

                if(op == IBM3151.OperationSpecifier.LogicalOR)
                {
                    if ((code & IBM3151.CharacterAttributes.Intense) == IBM3151.CharacterAttributes.Intense)
                        Console.ForegroundColor = ConsoleColor.Green;
                }

                if (op == IBM3151.OperationSpecifier.LogicalAND)
                {
                    if ((code & IBM3151.CharacterAttributes.Intense) != IBM3151.CharacterAttributes.Intense)
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    
                }

                return 3;
            }
            Console.Write('\u2190');
            return 0;
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
		
		public int BytesToRead { get { throw new NotImplementedException(); } }

        public int ReadByte()
        {
            var keyInfo = Console.ReadKey(true);
            return keyInfo.KeyChar;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            buffer[offset] = (byte)Console.ReadKey(true).KeyChar;
            return 1;
        }

        public void Dispose()
        {
            
        }
    }
}
