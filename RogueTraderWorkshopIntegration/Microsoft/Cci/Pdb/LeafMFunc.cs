namespace Microsoft.Cci.Pdb;

internal struct LeafMFunc
{
	internal uint rvtype;

	internal uint classtype;

	internal uint thistype;

	internal byte calltype;

	internal byte reserved;

	internal ushort parmcount;

	internal uint arglist;

	internal int thisadjust;
}
