namespace Microsoft.Cci.Pdb;

internal struct DbiSecCon
{
	internal short section;

	internal short pad1;

	internal int offset;

	internal int size;

	internal uint flags;

	internal short module;

	internal short pad2;

	internal uint dataCrc;

	internal uint relocCrc;

	internal DbiSecCon(BitAccess bits)
	{
		bits.ReadInt16(out section);
		bits.ReadInt16(out pad1);
		bits.ReadInt32(out offset);
		bits.ReadInt32(out size);
		bits.ReadUInt32(out flags);
		bits.ReadInt16(out module);
		bits.ReadInt16(out pad2);
		bits.ReadUInt32(out dataCrc);
		bits.ReadUInt32(out relocCrc);
	}
}
