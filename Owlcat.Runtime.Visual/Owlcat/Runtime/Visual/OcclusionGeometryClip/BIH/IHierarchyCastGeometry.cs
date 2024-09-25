using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

internal interface IHierarchyCastGeometry<TGeometry>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	ABox GetBounds();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IntersectsNode(ABox bounds);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IntersectsLeaf(TGeometry bounds);
}
