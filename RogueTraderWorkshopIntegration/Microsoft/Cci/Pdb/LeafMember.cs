namespace Microsoft.Cci.Pdb;

internal struct LeafMember
{
	internal ushort attr;

	internal uint index;

	internal byte[] offset;

	internal string name;
}
