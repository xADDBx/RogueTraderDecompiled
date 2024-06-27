namespace Microsoft.Cci.Pdb;

internal struct ManProcSym
{
	internal uint parent;

	internal uint end;

	internal uint next;

	internal uint len;

	internal uint dbgStart;

	internal uint dbgEnd;

	internal uint token;

	internal uint off;

	internal ushort seg;

	internal byte flags;

	internal ushort retReg;

	internal string name;
}
