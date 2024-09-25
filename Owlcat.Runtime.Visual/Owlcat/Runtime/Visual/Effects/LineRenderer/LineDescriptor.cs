using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

public struct LineDescriptor
{
	public int PositionCount;

	public int PositionsOffset;

	public float2 UvOffset;

	public float WidthScale;
}
