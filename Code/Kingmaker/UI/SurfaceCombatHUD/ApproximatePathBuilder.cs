using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct ApproximatePathBuilder
{
	private NativeList<ApproximatePathSegment> m_Segments;

	private bool m_OnePointReceived;

	private float3 m_LastPosition;

	public ApproximatePathBuilder(NativeList<ApproximatePathSegment> segments)
	{
		m_Segments = segments;
		m_OnePointReceived = false;
		m_LastPosition = default(float3);
	}

	public void StartLine()
	{
		m_Segments.Clear();
	}

	public void PushPoint(in SplinePoint point)
	{
		if (m_OnePointReceived)
		{
			if (!m_LastPosition.Equals(point.position))
			{
				float3 @float = point.position - m_LastPosition;
				float num = math.length(@float);
				ref NativeList<ApproximatePathSegment> segments = ref m_Segments;
				ApproximatePathSegment value = new ApproximatePathSegment
				{
					direction = @float / num,
					length = num,
					spatialDistanceAtEnd = point.spatialDistance
				};
				segments.Add(in value);
			}
		}
		else
		{
			ref NativeList<ApproximatePathSegment> segments2 = ref m_Segments;
			ApproximatePathSegment value = new ApproximatePathSegment
			{
				direction = point.position,
				length = 0f,
				spatialDistanceAtEnd = 0f
			};
			segments2.Add(in value);
		}
		m_OnePointReceived = true;
		m_LastPosition = point.position;
	}

	public void FinishLine()
	{
		if (m_Segments.Length < 2)
		{
			m_Segments.Clear();
		}
	}
}
