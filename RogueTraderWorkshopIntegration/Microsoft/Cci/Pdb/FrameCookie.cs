namespace Microsoft.Cci.Pdb;

internal struct FrameCookie
{
	internal int off;

	internal ushort reg;

	internal int cookietype;

	internal byte flags;
}
