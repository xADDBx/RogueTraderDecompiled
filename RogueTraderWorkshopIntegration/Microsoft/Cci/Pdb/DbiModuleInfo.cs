namespace Microsoft.Cci.Pdb;

internal class DbiModuleInfo
{
	internal int opened;

	internal ushort flags;

	internal short stream;

	internal int cbSyms;

	internal int cbOldLines;

	internal int cbLines;

	internal short files;

	internal short pad1;

	internal uint offsets;

	internal int niSource;

	internal int niCompiler;

	internal string moduleName;

	internal string objectName;

	internal DbiModuleInfo(BitAccess bits, bool readStrings)
	{
		bits.ReadInt32(out opened);
		new DbiSecCon(bits);
		bits.ReadUInt16(out flags);
		bits.ReadInt16(out stream);
		bits.ReadInt32(out cbSyms);
		bits.ReadInt32(out cbOldLines);
		bits.ReadInt32(out cbLines);
		bits.ReadInt16(out files);
		bits.ReadInt16(out pad1);
		bits.ReadUInt32(out offsets);
		bits.ReadInt32(out niSource);
		bits.ReadInt32(out niCompiler);
		if (readStrings)
		{
			bits.ReadCString(out moduleName);
			bits.ReadCString(out objectName);
		}
		else
		{
			bits.SkipCString(out moduleName);
			bits.SkipCString(out objectName);
		}
		bits.Align(4);
	}
}
