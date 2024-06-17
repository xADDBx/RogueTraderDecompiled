using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

[BurstCompile]
public struct CompositeLineUpdateJob : IJobLineRenderer, IJobParallelFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	private NativeArray<Point> m_Points;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Point> InputPoints;

	public NativeArray<Point> Points
	{
		get
		{
			return m_Points;
		}
		set
		{
			m_Points = value;
		}
	}

	public NativeArray<LineDescriptor> Lines { get; set; }

	public void Execute(int index)
	{
		NativeArray<Point> points = Points;
		LineDescriptor lineDescriptor = Lines[index];
		int positionsOffset = lineDescriptor.PositionsOffset;
		int num = lineDescriptor.PositionsOffset + lineDescriptor.PositionCount;
		for (int i = positionsOffset; i < num; i++)
		{
			points[i] = InputPoints[i];
		}
	}
}
