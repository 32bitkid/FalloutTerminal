using System;
using System.Collections.Generic;
using System.Text;

namespace FalloutTerminal.Communications
{
    public class FakeConsoleSerialPort : ISerialConnection
    {
        public void Write(byte[] buffer, int offset, int count)
        {
            Console.Write(Encoding.Default.GetString(buffer, offset, count));
        }

        public void Write(string text)
        {
            Console.Write(text);
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
            throw new NotImplementedException();
        }
    }
}
