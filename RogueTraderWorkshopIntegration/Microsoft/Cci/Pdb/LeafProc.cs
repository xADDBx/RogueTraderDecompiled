namespace Microsoft.Cci.Pdb;

internal struct LeafProc
{
	internal uint rvtype;

	internal byte calltype;

	internal byte reserved;

	internal ushort parmcount;

	internal uint arglist;
}
