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
		return GridNavmeshModifier.CalculateGraphSpaceBounds(t, m_Bounds);
	}

	public override void NotifyUpdated()
	{
		base.NotifyUpdated();
		RecalculateBoundsAndTop();
	}
}
