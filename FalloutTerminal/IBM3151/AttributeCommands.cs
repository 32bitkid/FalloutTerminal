
using System;
using System.Text;

namespace FalloutTerminal
{

	[Flags]
	public enum CharacterAttributes
	{
		None = 0,
		Reverse = 1,
		Underline = 2,
		Blink = 4,
		Intense = 8,
		Hidden = 16,
		Op = 32,
		NoOp = 64,
	}
	
	public enum OperationSpecifier {
		Replacement = (3 << 5),
		LogicalOR = (3 << 5) | 1,
		LogicalAND = (3 << 5) | 2,
	}

	public partial class SerialTerminal
	{
		public bool Intense {
			set {
				
				byte pa = (byte)(CharacterAttributes.Intense | CharacterAttributes.Op);
				byte op = (byte)((value) ? OperationSpecifier.LogicalOR : OperationSpecifier.LogicalAND);
				                 
				if(value) 
					Write(new byte[] { Ascii.ESC, Ascii.Four, pa, op }, 0, 4);			
				else
					Write(new byte[] { Ascii.ESC, Ascii.Four, (byte)~pa, op }, 0, 4);			
			}
		}
		
		public string SetCharacterAttribute(CharacterAttributes attributes) {
			var pa = (byte)(attributes | CharacterAttributes.NoOp);
			return Encoding.ASCII.GetString(new byte[] {Ascii.ESC, Ascii.Four, pa });
		}
	}
}
