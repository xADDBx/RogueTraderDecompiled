namespace Microsoft.Cci.Pdb;

internal class PdbSynchronizationInformation
{
	internal uint kickoffMethodToken;

	internal uint generatedCatchHandlerIlOffset;

	internal PdbSynchronizationPoint[] synchronizationPoints;

	public uint GeneratedCatchHandlerOffset => generatedCatchHandlerIlOffset;

	internal PdbSynchronizationInformation(BitAccess bits)
	{
		bits.ReadUInt32(out kickoffMethodToken);
		bits.ReadUInt32(out generatedCatchHandlerIlOffset);
		bits.ReadUInt32(out var value);
		synchronizationPoints = new PdbSynchronizationPoint[value];
		for (uint num = 0u; num < value; num++)
		{
			synchronizationPoints[num] = new PdbSynchronizationPoint(bits);
		}
	}
}
