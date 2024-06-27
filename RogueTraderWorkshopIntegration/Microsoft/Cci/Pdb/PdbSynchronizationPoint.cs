namespace Microsoft.Cci.Pdb;

internal class PdbSynchronizationPoint
{
	internal uint synchronizeOffset;

	internal uint continuationMethodToken;

	internal uint continuationOffset;

	public uint SynchronizeOffset => synchronizeOffset;

	public uint ContinuationOffset => continuationOffset;

	internal PdbSynchronizationPoint(BitAccess bits)
	{
		bits.ReadUInt32(out synchronizeOffset);
		bits.ReadUInt32(out continuationMethodToken);
		bits.ReadUInt32(out continuationOffset);
	}
}
