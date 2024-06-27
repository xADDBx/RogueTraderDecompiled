namespace Microsoft.Cci.Pdb;

internal struct SectionSym
{
	internal ushort isec;

	internal byte align;

	internal byte bReserved;

	internal uint rva;

	internal uint cb;

	internal uint characteristics;

	internal string name;
}
