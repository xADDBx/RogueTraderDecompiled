using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal readonly struct PathSplinePointMetaData
{
	public readonly int spatialDistance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PathSplinePointMetaData(int spatialDistance)
	{
		this.spatialDistance = spatialDistance;
	}
}
