namespace Microsoft.Cci.Pdb;

internal struct CallsiteInfo
{
	internal int off;

	internal ushort ect;

	internal ushort pad0;

	internal uint typind;
}
