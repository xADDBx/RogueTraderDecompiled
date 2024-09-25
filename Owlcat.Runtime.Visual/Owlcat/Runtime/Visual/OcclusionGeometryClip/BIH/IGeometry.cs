using System.Runtime.CompilerServices;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

internal interface IGeometry
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	ABox GetBounds();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	(int begin, int end) GetIndexRange();
}
