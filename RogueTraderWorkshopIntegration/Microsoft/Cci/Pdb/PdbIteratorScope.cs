namespace Microsoft.Cci.Pdb;

internal sealed class PdbIteratorScope : ILocalScope
{
	private uint offset;

	private uint length;

	public uint Offset => offset;

	public uint Length => length;

	internal PdbIteratorScope(uint offset, uint length)
	{
		this.offset = offset;
		this.length = length;
	}
}
