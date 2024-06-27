namespace Microsoft.Cci.Pdb;

internal class PdbSlot
{
	internal uint slot;

	internal uint typeToken;

	internal string name;

	internal ushort flags;

	internal PdbSlot(uint slot, uint typeToken, string name, ushort flags)
	{
		this.slot = slot;
		this.typeToken = typeToken;
		this.name = name;
		this.flags = flags;
	}

	internal PdbSlot(BitAccess bits)
	{
		AttrSlotSym attrSlotSym = default(AttrSlotSym);
		bits.ReadUInt32(out attrSlotSym.index);
		bits.ReadUInt32(out attrSlotSym.typind);
		bits.ReadUInt32(out attrSlotSym.offCod);
		bits.ReadUInt16(out attrSlotSym.segCod);
		bits.ReadUInt16(out attrSlotSym.flags);
		bits.ReadCString(out attrSlotSym.name);
		slot = attrSlotSym.index;
		typeToken = attrSlotSym.typind;
		name = attrSlotSym.name;
		flags = attrSlotSym.flags;
	}
}
