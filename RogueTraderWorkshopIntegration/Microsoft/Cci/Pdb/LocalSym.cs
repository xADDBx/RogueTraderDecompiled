namespace Microsoft.Cci.Pdb;

internal struct LocalSym
{
	internal uint id;

	internal uint typind;

	internal ushort flags;

	internal uint idParent;

	internal uint offParent;

	internal uint expr;

	internal uint pad0;

	internal uint pad1;

	internal string name;
}
