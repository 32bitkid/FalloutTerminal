
using System;

namespace FalloutTerminal.Communications
{
	public abstract class Ascii {
		public const byte NUL = 0;
		public const byte SOH = 1;
		public const byte STX = 2;
		public const byte ETX = 3;
		public const byte EOT = 4;
		public const byte ENQ = 5;
		public const byte ACK = 6;
		public const byte BEL = 7;
		public const byte BS = 8;
		public const byte HT = 9;
		public const byte LF = 10;
		public const byte VT = 11;
		public const byte FF = 12;
		public const byte CR = 13;
		public const byte SO = 14;
		public const byte SI = 15;
		public const byte DLE = 16;
		public const byte DC1 = 17;
		public const byte DC2 = 18;
		public const byte DC3 = 19;
		public const byte DC4 = 20;
		public const byte NAK = 21;
		public const byte SYN = 22;
		public const byte ETB = 23;
		public const byte CAN = 24;
		public const byte EM = 25;
		public const byte SUB = 26;
		public const byte ESC = 27;
		public const byte FS = 28;
		public const byte GS = 29;
		public const byte RS = 30;
		public const byte US = 31;
		public const byte SP = 32;
		public const byte ExclaimationPoint = 33;
		public const byte DoubleQuote = 34;
		public const byte PoundSign = 35;
		public const byte DollarSign = 36;
		public const byte PercentSign = 37;
		public const byte AndSIgn = 38;
		public const byte SingleQuote = 39;
		public const byte OpenParentheses = 40;
		public const byte CloseParentheses = 41;
		public const byte Star = 42;
		public const byte Plus = 43;
		public const byte Comma = 44;
		public const byte Dash = 45;
		public const byte Peroid = 46;
		public const byte Slash = 47;
		public const byte Zero = 48;
		public const byte One = 49;
		public const byte Two = 50;
		public const byte Three = 51;
		public const byte Four = 52;
		public const byte Five = 53;
		public const byte Six = 54;
		public const byte Seven = 55;
		public const byte Eight = 56;
		public const byte Nine = 57;
		public const byte Colon = 58;
		public const byte Semicolon = 59;
		public const byte LessThan = 60;
		public const byte Equals = 61;
		public const byte GreaterThan = 62;
		public const byte QuestionMark = 63;
		public const byte At = 64;
		public const byte A = 65;
		public const byte B = 66;
		public const byte C = 67;
		public const byte D = 68;
		public const byte E = 69;
		public const byte F = 70;
		public const byte G = 71;
		public const byte H = 72;
		public const byte I = 73;
		public const byte J = 74;
		public const byte K = 75;
		public const byte L = 76;
		public const byte M = 77;
		public const byte N = 78;
		public const byte O = 79;
		public const byte P = 80;
		public const byte Q = 81;
		public const byte R = 82;
		public const byte S = 83;
		public const byte T = 84;
		public const byte U = 85;
		public const byte V = 86;
		public const byte W = 87;
		public const byte X = 88;
		public const byte Y = 89;
		public const byte Z = 90;
		public const byte OpenBracket = 91;
		public const byte Backslash = 92;
		public const byte CloseBracket = 93;
		public const byte Caret = 94;
		public const byte Underscore = 95;
		public const byte Backtick = 96;
		public const byte a = 97;
		public const byte b = 98;
		public const byte c = 99;
		public const byte d = 100;
		public const byte e = 101;
		public const byte f = 102;
		public const byte g = 103;
		public const byte h = 104;
		public const byte i = 105;
		public const byte j = 106;
		public const byte k = 107;
		public const byte l = 108;
		public const byte m = 109;
		public const byte n = 110;
		public const byte o = 111;
		public const byte p = 112;
		public const byte q = 113;
		public const byte r = 114;
		public const byte s = 115;
		public const byte t = 116;
		public const byte u = 117;
		public const byte v = 118;
		public const byte w = 119;
		public const byte x = 120;
		public const byte y = 121;
		public const byte z = 122;
		public const byte OpenBrace = 123;
		public const byte Bar = 124;
		public const byte CloseBrace = 125;
		public const byte Tilde = 126;
		public const byte DEL = 127;
		
		public const byte Null = NUL;
		public const byte StartOfHeader = SOH;
		public const byte StartOfText = STX;
		public const byte EndOfText = ETX;
		public const byte EndOfTransmission = EOT;
		public const byte Enquiry = ENQ;
		public const byte Acknowledge = ACK;
		public const byte Bell = BEL;
		public const byte Backspace = BS;
		public const byte HorizontalTab = HT;
		public const byte LineFeed = LF;
		public const byte VerticalTab = VT;
		public const byte FormFeed = FF;
		public const byte CarriageReturn = CR;
		public const byte ShiftOut = SO;
		public const byte ShiftIn = SI;
		public const byte DataLinkEscape = DLE;
		public const byte DeviceControl1 = DC1;
		public const byte DeviceControl2 = DC2; 
		public const byte DeviceControl3 = DC3;
		public const byte DeviceControl4 = DC4;
		public const byte NegativeAcknowledge = NAK;
		public const byte Synchronous = SYN;
		public const byte EndOfTransmissionBlock = ETB;
		public const byte Cancel = CAN;
		public const byte EndOfMedium = EM;
		public const byte Substitute = SUB;
		public const byte Escape = ESC;
		public const byte FileSeparator = FS;
		public const byte GroupSeparator = GS;
		public const byte RecordSeparator = RS;
		public const byte UnitSeparator = US;
		public const byte Delete = DEL;		
		public const byte Space = SP;
	}
}