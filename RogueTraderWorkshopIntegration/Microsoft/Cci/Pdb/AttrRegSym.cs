namespace Microsoft.Cci.Pdb;

internal struct AttrRegSym
{
	internal uint typind;

	internal uint offCod;

	internal ushort segCod;

	internal ushort flags;

	internal ushort reg;

	internal string name;
}
