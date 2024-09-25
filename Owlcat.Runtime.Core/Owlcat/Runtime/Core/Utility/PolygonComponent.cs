using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Math;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public abstract class PolygonComponent : MonoBehaviour
{
	[SerializeField]
	[NotNull]
	private List<Vector3> m_Points = new List<Vector3>();

	[SerializeField]
	private Color m_GizmoColor = Color.green;

	[SerializeField]
	private bool m_Closed;

	public float PointYShift;

	private Vector3[] m_TransformedPoints;

	private Bounds m_TransforedBounds;

	public List<Vector3> Points => m_Points;

	public Vector3[] TransformedPoints
	{
		get
		{
			if (base.transform.hasChanged || m_TransformedPoints == null || m_TransformedPoints.Length != m_Points.Count)
			{
				m_TransformedPoints = (from v in m_Points.EmptyIfNull()
					select base.transform.TransformPoint(v)).ToArray();
				base.transform.hasChanged = false;
			}
			return m_TransformedPoints;
		}
	}

	public Bounds TransformedBound
	{
		get
		{
			Bounds result = default(Bounds);
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			float num5 = float.MinValue;
			float num6 = float.MinValue;
			Vector3[] transformedPoints = TransformedPoints;
			for (int i = 0; i < transformedPoints.Length; i++)
			{
				Vector3 vector = transformedPoints[i];
				if (vector.x < num)
				{
					num = vector.x;
				}
				if (vector.y < num2)
				{
					num = vector.y;
				}
				if (vector.z < num3)
				{
					num = vector.z;
				}
				if (vector.x > num4)
				{
					num4 = vector.x;
				}
				if (vector.y > num5)
				{
					num4 = vector.y;
				}
				if (vector.z > num6)
				{
					num4 = vector.z;
				}
			}
			result.SetMinMax(new Vector3(num, num2, num3), new Vector3(num4, num5, num6));
			return result;
		}
	}

	public bool Closed
	{
		get
		{
			return m_Closed;
		}
		set
		{
			m_Closed = value;
		}
	}

	public virtual Color GizmoColor
	{
		get
		{
			return m_GizmoColor;
		}
		set
		{
			m_GizmoColor = value;
		}
	}

	public virtual bool NeedsMeshCreation => false;

	protected virtual void OnDrawGizmosSelected()
	{
		if (TransformedPoints.Length >= 2)
		{
			Gizmos.color = GizmoColor;
			int num = (m_Closed ? TransformedPoints.Length : (TransformedPoints.Length - 1));
			for (int i = 0; i < num; i++)
			{
				Vector3 from = TransformedPoints[i];
				Vector3 to = TransformedPoints[(i + 1) % TransformedPoints.Length];
				Gizmos.DrawLine(from, to);
			}
		}
	}

	public Vector3[] GetPoints()
	{
		return m_Points.ToArray();
	}

	public void SetPointAbsolute(Vector3 point, int index)
	{
		m_TransformedPoints[index] = point;
		m_Points[index] = base.transform.InverseTransformPoint(point);
	}

	public void AddPointAtAbsolute(int index, Vector3 point)
	{
		m_Points.Insert(index, base.transform.InverseTransformPoint(point));
		m_TransformedPoints = null;
	}

	public void DeletePoint(int index)
	{
		m_Points.RemoveAt(index);
		m_TransformedPoints = null;
	}

	public void Reverse()
	{
		m_Points.Reverse();
		m_TransformedPoints = null;
	}

	public bool ContainsPointXZ(Vector3 worldPos)
	{
		return Polygon.ContainsPointXZ(TransformedPoints, worldPos);
	}
}
