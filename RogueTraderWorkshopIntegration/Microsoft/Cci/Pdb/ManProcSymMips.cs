namespace Microsoft.Cci.Pdb;

internal struct ManProcSymMips
{
	internal uint parent;

	internal uint end;

	internal uint next;

	internal uint len;

	internal uint dbgStart;

	internal uint dbgEnd;

	internal uint regSave;

	internal uint fpSave;

	internal uint intOff;

	internal uint fpOff;

	internal uint token;

	internal uint off;

	internal ushort seg;

	internal byte retReg;

	internal byte frameReg;

	internal string name;
}