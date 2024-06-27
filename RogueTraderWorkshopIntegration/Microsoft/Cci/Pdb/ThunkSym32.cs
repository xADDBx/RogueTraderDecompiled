namespace Microsoft.Cci.Pdb;

internal struct ThunkSym32
{
	internal uint parent;

	internal uint end;

	internal uint next;

	internal uint off;

	internal ushort seg;

	internal ushort len;

	internal byte ord;

	internal string name;

	internal byte[] variant;
}
