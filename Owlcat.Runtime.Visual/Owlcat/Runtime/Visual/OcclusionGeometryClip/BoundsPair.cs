namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

internal readonly struct BoundsPair
{
	public readonly ABox axisAligned;

	public readonly OBox oriented;

	public BoundsPair(ABox axisAligned, OBox oriented)
	{
		this.axisAligned = axisAligned;
		this.oriented = oriented;
	}

	public void Deconstruct(out ABox a, out OBox b)
	{
		a = axisAligned;
		b = oriented;
	}
}
