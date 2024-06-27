namespace Microsoft.Cci.Pdb;

internal struct LeafClass
{
	internal ushort count;

	internal ushort property;

	internal uint field;

	internal uint derived;

	internal uint vshape;

	internal byte[] data;

	internal string name;
}
