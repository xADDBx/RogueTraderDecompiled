using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
internal readonly struct OccluderGeometry : IGeometry
{
	public readonly int rendererIndicesArrayBegin;

	public readonly int rendererIndicesArrayEnd;

	public readonly ABox aBox;

	public readonly OBox oBox;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OccluderGeometry(int rendererIndicesArrayBegin, int rendererIndicesArrayEnd, ABox aBox, OBox oBox)
	{
		this.rendererIndicesArrayBegin = rendererIndicesArrayBegin;
		this.rendererIndicesArrayEnd = rendererIndicesArrayEnd;
		this.aBox = aBox;
		this.oBox = oBox;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ABox GetBounds()
	{
		return aBox;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (int begin, int end) GetIndexRange()
	{
		return (begin: rendererIndicesArrayBegin, end: rendererIndicesArrayEnd);
	}

	public override string ToString()
	{
		return $"{{rendererIndicesArrayBegin:{rendererIndicesArrayBegin}, rendererIndicesArrayEnd:{rendererIndicesArrayEnd}, abox:{aBox}, obox:{oBox}}}";
	}
}
