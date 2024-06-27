using System.IO;
using System.Linq;

namespace Microsoft.Cci.Pdb;

internal class PdbFileHeader
{
	private readonly byte[] windowsPdbMagic = new byte[32]
	{
		77, 105, 99, 114, 111, 115, 111, 102, 116, 32,
		67, 47, 67, 43, 43, 32, 77, 83, 70, 32,
		55, 46, 48, 48, 13, 10, 26, 68, 83, 0,
		0, 0
	};

	internal readonly byte[] magic;

	internal readonly int pageSize;

	internal int freePageMap;

	internal int pagesUsed;

	internal int directorySize;

	internal readonly int zero;

	internal int[] directoryRoot;

	internal PdbFileHeader(Stream reader, BitAccess bits)
	{
		bits.MinCapacity(56);
		reader.Seek(0L, SeekOrigin.Begin);
		bits.FillBuffer(reader, 52);
		magic = new byte[32];
		bits.ReadBytes(magic);
		bits.ReadInt32(out pageSize);
		bits.ReadInt32(out freePageMap);
		bits.ReadInt32(out pagesUsed);
		bits.ReadInt32(out directorySize);
		bits.ReadInt32(out zero);
		if (!magic.SequenceEqual(windowsPdbMagic))
		{
			throw new PdbException("The PDB file is not recognized as a Windows PDB file");
		}
		int num = ((directorySize + pageSize - 1) / pageSize * 4 + pageSize - 1) / pageSize;
		directoryRoot = new int[num];
		bits.FillBuffer(reader, num * 4);
		bits.ReadInt32(directoryRoot);
	}
}
