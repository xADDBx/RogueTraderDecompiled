namespace Microsoft.Cci.Pdb;

internal struct FrameRelSym
{
	internal int off;

	internal uint typind;

	internal uint offCod;

	internal ushort segCod;

	internal ushort flags;

	internal string name;
}
