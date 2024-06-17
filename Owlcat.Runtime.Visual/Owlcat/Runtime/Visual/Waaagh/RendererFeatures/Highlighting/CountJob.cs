using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[BurstCompile]
internal struct CountJob : IJob
{
	[ReadOnly]
	public NativeSlice<BoundsVisibility> Bounds;

	[WriteOnly]
	public NativeReference<int> Count;

	public void Execute()
	{
		int num = 0;
		foreach (BoundsVisibility bound in Bounds)
		{
			if (bound.Visibility != TestPlanesResults.Outside)
			{
				num++;
			}
		}
		Count.Value = num;
	}
}
