namespace Microsoft.Cci.Pdb;

internal struct DbiHeader
{
	internal int sig;

	internal int ver;

	internal int age;

	internal short gssymStream;

	internal ushort vers;

	internal short pssymStream;

	internal ushort pdbver;

	internal short symrecStream;

	internal ushort pdbver2;

	internal int gpmodiSize;

	internal int secconSize;

	internal int secmapSize;

	internal int filinfSize;

	internal int tsmapSize;

	internal int mfcIndex;

	internal int dbghdrSize;

	internal int ecinfoSize;

	internal ushort flags;

	internal ushort machine;

	internal int reserved;

	internal DbiHeader(BitAccess bits)
	{
		bits.ReadInt32(out sig);
		bits.ReadInt32(out ver);
		bits.ReadInt32(out age);
		bits.ReadInt16(out gssymStream);
		bits.ReadUInt16(out vers);
		bits.ReadInt16(out pssymStream);
		bits.ReadUInt16(out pdbver);
		bits.ReadInt16(out symrecStream);
		bits.ReadUInt16(out pdbver2);
		bits.ReadInt32(out gpmodiSize);
		bits.ReadInt32(out secconSize);
		bits.ReadInt32(out secmapSize);
		bits.ReadInt32(out filinfSize);
		bits.ReadInt32(out tsmapSize);
		bits.ReadInt32(out mfcIndex);
		bits.ReadInt32(out dbghdrSize);
		bits.ReadInt32(out ecinfoSize);
		bits.ReadUInt16(out flags);
		bits.ReadUInt16(out machine);
		bits.ReadInt32(out reserved);
	}
}
