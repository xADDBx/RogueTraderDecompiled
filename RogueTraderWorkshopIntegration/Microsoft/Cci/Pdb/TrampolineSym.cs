namespace Microsoft.Cci.Pdb;

internal struct TrampolineSym
{
	internal ushort trampType;

	internal ushort cbThunk;

	internal uint offThunk;

	internal uint offTarget;

	internal ushort sectThunk;

	internal ushort sectTarget;
}
