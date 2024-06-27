using System;

namespace Microsoft.Cci.Pdb;

internal class PdbScope
{
	internal PdbConstant[] constants;

	internal PdbSlot[] slots;

	internal PdbScope[] scopes;

	internal string[] usedNamespaces;

	internal uint address;

	internal uint offset;

	internal uint length;

	internal PdbScope(uint address, uint offset, uint length, PdbSlot[] slots, PdbConstant[] constants, string[] usedNamespaces)
	{
		this.constants = constants;
		this.slots = slots;
		scopes = new PdbScope[0];
		this.usedNamespaces = usedNamespaces;
		this.address = address;
		this.offset = offset;
		this.length = length;
	}

	internal PdbScope(uint address, uint length, PdbSlot[] slots, PdbConstant[] constants, string[] usedNamespaces)
		: this(address, 0u, length, slots, constants, usedNamespaces)
	{
	}

	internal PdbScope(uint funcOffset, BlockSym32 block, BitAccess bits, out uint typind)
	{
		address = block.off;
		offset = block.off - funcOffset;
		length = block.len;
		typind = 0u;
		PdbFunction.CountScopesAndSlots(bits, block.end, out var num, out var num2, out var num3, out var num4);
		constants = new PdbConstant[num];
		scopes = new PdbScope[num2];
		slots = new PdbSlot[num3];
		usedNamespaces = new string[num4];
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		while (bits.Position < block.end)
		{
			bits.ReadUInt16(out var value);
			int position = bits.Position;
			int position2 = bits.Position + value;
			bits.Position = position;
			bits.ReadUInt16(out var value2);
			switch ((SYM)value2)
			{
			case SYM.S_BLOCK32:
			{
				BlockSym32 block2 = default(BlockSym32);
				bits.ReadUInt32(out block2.parent);
				bits.ReadUInt32(out block2.end);
				bits.ReadUInt32(out block2.len);
				bits.ReadUInt32(out block2.off);
				bits.ReadUInt16(out block2.seg);
				bits.SkipCString(out block2.name);
				bits.Position = position2;
				scopes[num6++] = new PdbScope(funcOffset, block2, bits, out typind);
				break;
			}
			case SYM.S_MANSLOT:
				slots[num7++] = new PdbSlot(bits);
				bits.Position = position2;
				break;
			case SYM.S_UNAMESPACE:
				bits.ReadCString(out usedNamespaces[num8++]);
				bits.Position = position2;
				break;
			case SYM.S_END:
				bits.Position = position2;
				break;
			case SYM.S_MANCONSTANT:
				constants[num5++] = new PdbConstant(bits);
				bits.Position = position2;
				break;
			default:
				bits.Position = position2;
				break;
			}
		}
		if (bits.Position != block.end)
		{
			throw new Exception("Not at S_END");
		}
		bits.ReadUInt16(out var _);
		bits.ReadUInt16(out var value4);
		if (value4 != 6)
		{
			throw new Exception("Missing S_END");
		}
	}
}
