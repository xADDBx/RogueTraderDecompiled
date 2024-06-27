namespace Microsoft.Cci.Pdb;

internal struct FrameProcSym
{
	internal uint cbFrame;

	internal uint cbPad;

	internal uint offPad;

	internal uint cbSaveRegs;

	internal uint offExHdlr;

	internal ushort secExHdlr;

	internal uint flags;
}
