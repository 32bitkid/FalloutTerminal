using System;

namespace FalloutTerminal
{
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

	public partial class SerialTerminal
	{
		public MachineModes MachineMode {
			get {
				this.Write(new byte[] { Ascii.ESC, Ascii.SP, Ascii.Seven} ,0, 3);
				
				var length = ReadResponse(_buffer, 0);
				
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
				this.Write(new byte[] { Ascii.ESC, Ascii.SP, Ascii.Seven} ,0, 3);
				
				var length = ReadResponse(_buffer, 0);
				
				if(_buffer[0] != Ascii.ESC && _buffer[1] != Ascii.SP && _buffer[2] != Ascii.Seven)
					throw new Exception("Invalid Reply");
				
				return (OperatingModes)(_buffer[3] & 3);
			}
			
			set {
				throw new NotSupportedException();
			}
		}
		
		public string TerminalId { 
			get {
				Write(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.Six }, 0, 3);
				return string.Empty;
			}
		}
		
		public bool KeyboardLock { 
			get {
				throw new NotSupportedException();
			}
			
			set {
				if(value)
					this.Write(new byte[] { Ascii.ESC, Ascii.Colon }, 0, 2);
				else
					this.Write(new byte[] { Ascii.ESC, Ascii.Semicolon},0, 2);
			}
		}
		
		public void Reset() {
			this.Write(new byte[] { Ascii.ESC, Ascii.SP, Ascii.S }, 0, 3);
		}
		
		public void MoveCursor(CursorDirections direction) {
			switch(direction) {
			case CursorDirections.Home:
				this.Write(new byte[] { Ascii.ESC, Ascii.H }, 0, 2);
				return;
			case CursorDirections.Up:
				this.Write(new byte[] { Ascii.ESC, Ascii.A }, 0, 2);
				return;
			case CursorDirections.Down:
				this.Write(new byte[] { Ascii.ESC, Ascii.B }, 0, 2);
				return;
			case CursorDirections.Left:
				this.Write(new byte[] { Ascii.ESC, Ascii.C }, 0, 2);
				return;
			case CursorDirections.Right:
				this.Write(new byte[] { Ascii.ESC, Ascii.D }, 0, 2);
				return;
			}
			
		}
		
		public void Index() { Write(new byte[] { Ascii.ESC, Ascii.SP, Ascii.M }, 0, 3); }
		public void ReverseIndex() { Write(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.M }, 0, 3); } 
		
		public CursorPosition GetCursorPosition() {
			
			Write(new byte[] { Ascii.ESC, Ascii.Five },0,2 );
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
		
		public void SetCursorPosition(int row, int column) {
			if(row > 95 || column > 95) 
			{
				SetCursorPositionEx(row, column);
				return;
			}
			
			Write(new byte[] { Ascii.ESC, Ascii.Y, (byte)(row + 0x20), (byte)(column + 0x20)}, 0, 4);
		}
		
		public void SetCursorPositionEx(int row, int column) {
			byte rowLow = (byte)(((row - 1) % 32) + 0x20);
			byte rowHi = (byte)(((row - 1) / 32) + 0x20);
			byte colLow = (byte)(((column - 1) % 32) + 0x40);
			byte colHi = (byte)(((column - 1) / 32) + 0x20);
			
			Console.WriteLine(BitConverter.ToString(new byte[] { Ascii.ESC, Ascii.y, rowHi, rowLow, colHi, colLow}));
			Write(new byte[] { Ascii.ESC, Ascii.y, rowHi, rowLow, colHi, colLow}, 0, 6);
		}
		
		public void SetCursorPosition(CursorPosition position) {
			SetCursorPosition(position.Row, position.Column);
		}
		
		public void ClearAll() {
			Write(new byte[] { Ascii.ESC, Ascii.ExclaimationPoint, Ascii.L }, 0, 3);
		}
		
	}
}
