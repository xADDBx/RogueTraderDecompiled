namespace Microsoft.Cci.Pdb;

internal struct DiscardedSym
{
	internal CV_DISCARDED iscarded;

	internal uint fileid;

	internal uint linenum;

	internal byte[] data;
}
