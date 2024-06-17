using Kingmaker.Utility.DotNetExtensions;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class ThinCover : GridNavmeshModifier
{
	private Rect m_Bounds;

	protected override bool ShouldFixLayer => false;

	public float Top { get; private set; }

	public void Init()
	{
		Awake();
	}

	protected override void Awake()
	{
		base.Awake();
		RecalculateBoundsAndTop();
	}

	public void RecalculateBoundsAndTop()
	{
		if (base.isActiveAndEnabled)
		{
			Bounds bounds = m_Colliders.FirstItem()?.bounds ?? default(Bounds);
			for (int i = 1; i < m_Colliders.Length; i++)
			{
				Collider collider = m_Colliders[i];
				bounds.Encapsulate(collider.bounds);
			}
			m_Bounds = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
			Top = bounds.max.y;
		}
	}

	public override Rect GetBounds(GraphTransform t)
	{
		Vector3 point = new Vector3(m_Bounds.min.x, 0f, m_Bounds.min.y);
		Vector3 point2 = new Vector3(m_Bounds.max.x, 0f, m_Bounds.max.y);
		Vector3 vector = t.InverseTransform(point);
		Vector3 vector2 = t.InverseTransform(point2);
		return Rect.MinMaxRect(Mathf.Round(vector.x) - 0.04f, Mathf.Round(vector.z) - 0.04f, Mathf.Round(vector2.x) + 0.04f, Mathf.Round(vector2.z) + 0.04f);
	}

	public override void NotifyUpdated()
	{
		base.NotifyUpdated();
		RecalculateBoundsAndTop();
	}
}
