using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct BezierPoint
{
	public float coefficient1;

	public float coefficient2;

	public float coefficient3;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 Evaluate(in float3 p0, in float3 p1, in float3 p2)
	{
		return coefficient1 * p0 + coefficient2 * p1 + coefficient3 * p2;
	}
}
