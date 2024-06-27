namespace Microsoft.Cci.Pdb;

internal struct ProcSym32
{
	internal uint parent;

	internal uint end;

	internal uint next;

	internal uint len;

	internal uint dbgStart;

	internal uint dbgEnd;

	internal uint typind;

	internal uint off;

	internal ushort seg;

	internal byte flags;

	internal string name;
}
