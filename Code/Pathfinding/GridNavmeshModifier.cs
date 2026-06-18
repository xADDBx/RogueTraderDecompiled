using System.Linq;
using Kingmaker;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public class GridNavmeshModifier : NavmeshClipper, INodesTagProvider
{
	private const float MovingEps = 0.0001f;

	private const float UpdateDistance = 0.4f;

	private const float UpdateRotationDistance = 45f;

	private const float BoundsPadding = 0.04f;

	[SerializeField]
	[Tooltip("Мини-оптимизация: Если галка установлена, навмеш будет обновляться каждый кадр при движении объекта. Иначе он будет обновляться только на кадрах, когда объект останавливается после движения.")]
	private bool m_UpdateDuringMoving;

	private Transform m_Transform;

	protected Collider[] m_Colliders;

	private Vector3 m_LastUpdatedPosition;

	private Quaternion m_LastUpdatedRotation;

	private Rect m_LastUpdatedBounds;

	private Vector3 m_LastStopCheckPosition;

	private bool m_WasMoving;

	private int m_LastStopCheckStep = -1;

	private bool m_CachedWasStopped;

	private bool m_ForceUpdatePending;

	private Rect m_CachedBounds;

	private bool m_ForceOverrideWalkability;

	protected virtual bool ShouldFixLayer => true;

	private Rect Bounds
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

	public int Tag
	{
		get
		{
			if (!m_ForceOverrideWalkability)
			{
				return 0;
			}
			return 31;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_Transform = base.transform;
		m_Colliders = GetComponentsInChildren<Collider>();
		m_ForceOverrideWalkability = base.gameObject.layer == 18;
		if (ShouldFixLayer && base.gameObject.layer != 19 && base.gameObject.layer != 18)
		{
			PFLog.Pathfinding.Error(this, $"{base.name}: GridNavmeshModifier has invalid Layer ({(Layers)base.gameObject.layer})");
			base.gameObject.layer = 19;
		}
	}

	public override void NotifyUpdated()
	{
		m_LastStopCheckPosition = (m_LastUpdatedPosition = m_Transform.position);
		m_LastUpdatedRotation = m_Transform.rotation;
		m_LastUpdatedBounds = Bounds;
		m_ForceUpdatePending = false;
	}

	public override Rect GetBounds(GraphTransform t)
	{
		return CalculateGraphSpaceBounds(t, Bounds);
	}

	public Rect GetLastUpdatedBounds(GraphTransform t)
	{
		if (!(m_LastUpdatedBounds == default(Rect)))
		{
			return CalculateGraphSpaceBounds(t, m_LastUpdatedBounds);
		}
		return default(Rect);
	}

	public override bool RequiresUpdate()
	{
		if (m_ForceUpdatePending)
		{
			return true;
		}
		if (Quaternion.Angle(m_LastUpdatedRotation, m_Transform.rotation) > 45f)
		{
			return true;
		}
		if (m_UpdateDuringMoving)
		{
			return (m_Transform.position - m_LastUpdatedPosition).sqrMagnitude > 0.16000001f;
		}
		return WasStoppedThisFrame();
	}

	public override void ForceUpdate()
	{
		m_ForceUpdatePending = true;
		ForceUpdateColliderBounds();
	}

	private bool WasStoppedThisFrame()
	{
		int currentSystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		if (currentSystemStepIndex == m_LastStopCheckStep)
		{
			return m_CachedWasStopped;
		}
		m_LastStopCheckStep = currentSystemStepIndex;
		Vector3 position = m_Transform.position;
		bool flag = (position - m_LastStopCheckPosition).sqrMagnitude > 0.0001f;
		m_LastStopCheckPosition = position;
		m_CachedWasStopped = m_WasMoving && !flag;
		m_WasMoving = flag;
		return m_CachedWasStopped;
	}

	private void ForceUpdateColliderBounds()
	{
		Collider[] colliders = m_Colliders;
		foreach (Collider obj in colliders)
		{
			bool flag = obj.enabled;
			obj.enabled = false;
			obj.enabled = true;
			obj.enabled = flag;
		}
	}

	protected static Rect CalculateGraphSpaceBounds(GraphTransform t, Rect bounds)
	{
		Vector3 vector = t.InverseTransform(new Vector3(bounds.xMin, 0f, bounds.yMin));
		Vector3 vector2 = t.InverseTransform(new Vector3(bounds.xMax, 0f, bounds.yMax));
		return Rect.MinMaxRect(Mathf.Round(vector.x) - 0.04f, Mathf.Round(vector.z) - 0.04f, Mathf.Round(vector2.x) + 0.04f, Mathf.Round(vector2.z) + 0.04f);
	}
}
