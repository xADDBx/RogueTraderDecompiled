namespace Microsoft.Cci.Pdb;

internal struct AttrRegRel
{
	internal uint off;

	internal uint typind;

	internal ushort reg;

	internal uint offCod;

	internal ushort segCod;

	internal ushort flags;

	internal string name;
}
