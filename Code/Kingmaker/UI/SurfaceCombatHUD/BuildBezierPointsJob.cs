using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct BuildBezierPointsJob : IJob
{
	public int segmentsCount;

	[WriteOnly]
	public NativeArray<BezierPoint> points;

	public void Execute()
	{
		float num = 1f / (float)segmentsCount;
		for (int i = 0; i <= segmentsCount; i++)
		{
			float num2 = num * (float)i;
			float num3 = 1f - num2;
			points[i] = new BezierPoint
			{
				coefficient1 = num3 * num3,
				coefficient2 = 2f * num3 * num2,
				coefficient3 = num2 * num2
			};
		}
	}
}
