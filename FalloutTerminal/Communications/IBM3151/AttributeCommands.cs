
using System;
using System.Text;

namespace FalloutTerminal.Communications
{
	public partial class IBM3151 { 
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
			
			NotIntense = 23
		}
		
		public enum OperationSpecifier {
			Replacement = (3 << 5),
			LogicalOR = (3 << 5) | 1,
			LogicalAND = (3 << 5) | 2,
		}
		
		public partial struct Commands {		
			public static readonly string Intense = Encoding.ASCII.GetString(new byte[] {
				Ascii.ESC, Ascii.Four,
				(byte)(CharacterAttributes.Intense | CharacterAttributes.Op),
				(byte)OperationSpecifier.LogicalOR
			});
			
			public static readonly string NotIntense = Encoding.ASCII.GetString(new byte[] {
				Ascii.ESC, Ascii.Four,
				(byte)(CharacterAttributes.NotIntense | CharacterAttributes.Op),
				(byte)OperationSpecifier.LogicalAND
			});
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
}