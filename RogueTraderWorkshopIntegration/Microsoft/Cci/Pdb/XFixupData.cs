namespace Microsoft.Cci.Pdb;

internal struct XFixupData
{
	internal ushort wType;

	internal ushort wExtra;

	internal uint rva;

	internal uint rvaTarget;
}
