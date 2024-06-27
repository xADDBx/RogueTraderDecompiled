namespace Microsoft.Cci.Pdb;

internal struct DbiDbgHdr
{
	internal ushort snFPO;

	internal ushort snException;

	internal ushort snFixup;

	internal ushort snOmapToSrc;

	internal ushort snOmapFromSrc;

	internal ushort snSectionHdr;

	internal ushort snTokenRidMap;

	internal ushort snXdata;

	internal ushort snPdata;

	internal ushort snNewFPO;

	internal ushort snSectionHdrOrig;

	internal DbiDbgHdr(BitAccess bits)
	{
		bits.ReadUInt16(out snFPO);
		bits.ReadUInt16(out snException);
		bits.ReadUInt16(out snFixup);
		bits.ReadUInt16(out snOmapToSrc);
		bits.ReadUInt16(out snOmapFromSrc);
		bits.ReadUInt16(out snSectionHdr);
		bits.ReadUInt16(out snTokenRidMap);
		bits.ReadUInt16(out snXdata);
		bits.ReadUInt16(out snPdata);
		bits.ReadUInt16(out snNewFPO);
		bits.ReadUInt16(out snSectionHdrOrig);
	}
}
