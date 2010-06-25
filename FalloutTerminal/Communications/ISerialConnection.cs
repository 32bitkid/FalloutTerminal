using System;

namespace FalloutTerminal.Communications
{
    public interface ISerialConnection : IDisposable
    {
		event EventHandler<EventArgs> Restart;
		
        void Write(byte[] buffer, int offset, int count);
        void Write(String text);
        void WriteLine( string text );
        int ReadByte();
        int Read(byte[] buffer, int offset, int count);
		int BytesToRead { get; }
		
		string GetString();
    }
}
