namespace Microsoft.Cci.Pdb;

internal struct CoffGroupSym
{
	internal uint cb;

	internal uint characteristics;

	internal uint off;

	internal ushort seg;

	internal string name;
}
