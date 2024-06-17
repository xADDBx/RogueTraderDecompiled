using System.Linq;
using Kingmaker;
using Owlcat.Runtime.Core.Utility;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class GridNavmeshModifier : NavmeshClipper
{
	private const float UpdateDistance = 0.4f;

	private const float UpdateRotationDistance = 45f;

	protected Collider[] m_Colliders;

	private Transform m_Transform;

	private Vector3 m_LastPosition;

	private Quaternion m_LastRotation;

	private Rect m_CachedBounds;

	protected virtual bool ShouldFixLayer => true;

	public Rect Bounds
	{
		get
		{
			if (base.isActiveAndEnabled)
			{
				Bounds bounds = m_Colliders.FirstOrDefault()?.bounds ?? default(Bounds);
				for (int i = 1; i < m_Colliders.Length; i++)
				{
					Collider collider = m_Colliders[i];
					bounds.Encapsulate(collider.bounds);
				}
				m_CachedBounds = new Rect
				{
					xMin = bounds.min.x,
					yMin = bounds.min.z,
					xMax = bounds.max.x,
					yMax = bounds.max.z
				};
			}
			return m_CachedBounds;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_Transform = base.transform;
		m_Colliders = GetComponentsInChildren<Collider>();
		if (ShouldFixLayer && base.gameObject.layer != 19 && base.gameObject.layer != 18)
		{
			PFLog.Pathfinding.Error(this, $"{base.name}: GridNavmeshModifier has invalid Layer ({(Layers)base.gameObject.layer})");
			base.gameObject.layer = 19;
		}
	}

	public override void NotifyUpdated()
	{
		m_LastPosition = m_Transform.position;
		m_LastRotation = m_Transform.rotation;
	}

	public override Rect GetBounds(GraphTransform t)
	{
		return Bounds;
	}

	public override bool RequiresUpdate()
	{
		if (!((m_Transform.position - m_LastPosition).sqrMagnitude > 0.16000001f))
		{
			return Quaternion.Angle(m_LastRotation, m_Transform.rotation) > 45f;
		}
		return true;
	}

	public override void ForceUpdate()
	{
		m_LastPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
	}
}
