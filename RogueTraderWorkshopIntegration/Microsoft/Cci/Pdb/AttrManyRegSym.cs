namespace Microsoft.Cci.Pdb;

internal struct AttrManyRegSym
{
	internal uint typind;

	internal uint offCod;

	internal ushort segCod;

	internal ushort flags;

	internal byte count;

	internal byte[] reg;

	internal string name;
}
