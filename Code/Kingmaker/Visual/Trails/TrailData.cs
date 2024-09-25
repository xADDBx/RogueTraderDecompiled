using System.Collections.Generic;

namespace Kingmaker.Visual.Trails;

public class TrailData
{
	private List<TrailPoint> m_ControlPoints = new List<TrailPoint> { TrailPoint.Pop() };

	internal int LastAddedCount;

	public WidthOffset WidthOffset;

	public SpawnType SpawnType;

	public bool IsSmoothed;

	public float CurrentLength { get; private set; }

	internal void AddControlPoint(TrailPoint point)
	{
		m_ControlPoints.Insert(1, point);
	}

	internal int CalculatePointsCount(float minDistance)
	{
		if (IsSmoothed)
		{
			int num = 0;
			for (int i = 0; i < m_ControlPoints.Count - 1; i++)
			{
				float num2 = 0f;
				MathHelper.Vector3Distance(ref m_ControlPoints[i].Position, ref m_ControlPoints[i + 1].Position, out var result);
				while (num2 < result)
				{
					num2 += minDistance;
					num++;
				}
			}
		}
		return m_ControlPoints.Count;
	}
}
