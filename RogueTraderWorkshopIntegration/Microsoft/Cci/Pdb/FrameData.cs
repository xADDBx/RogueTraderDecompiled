namespace Microsoft.Cci.Pdb;

internal struct FrameData
{
	internal uint ulRvaStart;

	internal uint cbBlock;

	internal uint cbLocals;

	internal uint cbParams;

	internal uint cbStkMax;

	internal uint frameFunc;

	internal ushort cbProlog;

	internal ushort cbSavedRegs;

	internal uint flags;
}
