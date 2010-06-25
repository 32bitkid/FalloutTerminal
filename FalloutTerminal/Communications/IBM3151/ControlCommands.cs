using System;
using System.Text;

namespace FalloutTerminal.Communications
{
	public partial class IBM3151 { 
		public enum MachineModes {
			IBM_3151 = 0,
			IBM_3101 = 1,
			ADM_3A = 2,
			ADM_5 = 3,
			ADDS_VP_A2 = 4,
			HZ_1500 = 5,
			TVI_910 = 6,
			TVI_925E = 112,
			TVI_920 = 113,
			Unknown = -1,
		}
		
		public enum OperatingModes { 
			Echo = 0,
			Character = 1,
			Block = 2,
			Reserved = 3,
		}
		
		public enum CursorDirections { 
			Home, Up, Down, Left, Right,
		}
		
		public struct CursorPosition {
			public int Row;
			public int Column;
			
			public override string ToString ()
			{
				return string.Format("[CursorPosition {0},{1}]", Row, Column);
			}
	
		}
		
		public abstract partial class Commands {		
			public readonly static string LockKeyboard = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.Colon });
			public readonly static string UnlockKeyboard = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.Semicolon});
			public readonly static string Reset = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.SP, Ascii.S });
			public readonly static string CursorHome = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.H });
			public readonly static string CursorUp = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.A });
			public readonly static string CursorDown = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.B });
			public readonly static string CursorLeft = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.C });
			public readonly static string CursorRight = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.D });
			public readonly static string Index = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.SP, Ascii.M });
			public readonly static string ReverseIndex = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.M });
			public readonly static string GetCursor = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.Five });
			public readonly static string ClearAll = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.L });
			public readonly static string GetTerminalId = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.Six });
			public readonly static string Prompt = Encoding.ASCII.GetString(new byte[] { Ascii.CR, Ascii.ESC, Ascii.I, Ascii.GreaterThan });
			public readonly static string ClearEOL = Encoding.ASCII.GetString(new byte[] { Ascii.ESC, Ascii.I });
            public readonly static string Beep = Encoding.ASCII.GetString(new [] { Ascii.Bell }, 0, 1);

            public static string SetCursorPosition(int row, int column)
            {
                if (row > 95 || column > 95)
                    return SetCursorPositionEx(row, column);

                return Encoding.ASCII.GetString(new [] { Ascii.ESC, Ascii.Y, (byte)((row + 0x1F)), (byte)(column + 0x1F) }, 0, 4);
            }

            public static string SetCursorPositionEx(int row, int column)
            {
                var rowLow = (byte)(((row - 1) % 32) + 0x20);
                var rowHi = (byte)(((row - 1) / 32) + 0x20);
                var colLow = (byte)(((column - 1) % 32) + 0x40);
                var colHi = (byte)(((column - 1) / 32) + 0x20);

                return Encoding.ASCII.GetString(new [] { Ascii.ESC, Ascii.y, rowHi, rowLow, colHi, colLow }, 0, 6);
            }

            public static string SetCursorPosition(CursorPosition position)
            {
                return SetCursorPosition(position.Row, position.Column);
            }	
		}
		
	
		public partial class SerialTerminal
		{
			public MachineModes MachineMode {
				get {
					Write(new[] { Ascii.ESC, Ascii.SP, Ascii.Seven} ,0, 3);
					
					ReadResponse(_buffer, 0);
					
					if(_buffer[0] != Ascii.ESC && _buffer[1] != Ascii.SP && _buffer[2] != Ascii.Seven)
						throw new Exception("Invalid Reply");
					
					if((_buffer[3] & (1<<6)) != 0)
						return (MachineModes)((_buffer[3] >> 2) & 7);
					
					return (MachineModes)(((_buffer[3] & 28) << 2) | (_buffer[4] & 15));
				}
				
				set { 
					throw new NotSupportedException();
				}
			}
			
			public OperatingModes OperatingMode {
				get {
					Write(new[] { Ascii.ESC, Ascii.SP, Ascii.Seven} ,0, 3);
					
					ReadResponse(_buffer, 0);
					
					if(_buffer[0] != Ascii.ESC && _buffer[1] != Ascii.SP && _buffer[2] != Ascii.Seven)
						throw new Exception("Invalid Reply");
					
					return (OperatingModes)(_buffer[3] & 3);
				}
				
				set {
					Write(new[] { Ascii.ESC, Ascii.SP, Ascii.Seven} ,0, 3);
					
					var length = ReadResponse(_buffer, 0);
					
					if(_buffer[0] != Ascii.ESC && _buffer[1] != Ascii.SP && _buffer[2] != Ascii.Seven)
						throw new Exception("Invalid Reply");
					
					_buffer[2] = Ascii.Nine;
					_buffer[3] = (byte)((_buffer[3] & ~3) | ((byte)value));
					
					Write(_buffer, 0, length);
				}
			}
			
			public string TerminalId { 
				get {
					Write(Commands.GetTerminalId);
					var length = ReadResponse(_buffer, 0);
					return string.Empty;
				}
			}
			
			public string MoveCursor(CursorDirections direction) {
				switch(direction) {
				case CursorDirections.Home:
					return Commands.CursorHome;
				case CursorDirections.Up:
					return Commands.CursorUp;
				case CursorDirections.Down:
					return Commands.CursorDown;
				case CursorDirections.Left:
					return Commands.CursorLeft;
				case CursorDirections.Right:
					return Commands.CursorRight;
				}
				throw new NotSupportedException();
			}
			
			
			public CursorPosition GetCursorPosition() {
				Write( Commands.GetCursor);
				ReadResponse(_buffer, 0);
				if(_buffer[1] == Ascii.Y)
					return new CursorPosition() { Row = _buffer[2] - 0x1F, Column = _buffer[3] - 0x1F };
				
				if(_buffer[1] == Ascii.y)
					return new CursorPosition() {
						Row = ((_buffer[2] - 0x20) * 32) + (_buffer[3] - 0x20) + 1,
						Column = ((_buffer[4] - 0x20) * 32) + (_buffer[5] - 0x40) + 1
					};
				
				throw new NotSupportedException("Unknown Reply");
			}
		}
	}
}
