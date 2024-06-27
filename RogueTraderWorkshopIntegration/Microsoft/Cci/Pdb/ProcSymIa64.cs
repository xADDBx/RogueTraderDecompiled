namespace Microsoft.Cci.Pdb;

internal struct ProcSymIa64
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

	internal ushort retReg;

	internal byte flags;

	internal string name;
}
