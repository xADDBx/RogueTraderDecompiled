using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.View;

public class ViewInterpolationHelper
{
	private readonly AbstractUnitEntityView m_View;

	private bool m_ForceUpdatePosition;

	private Vector3 m_InterpolationFrom;

	private Vector3 m_InterpolationTo;

	private float m_PreviousOrientation;

	public float TurretInterpolatedOrientation { get; private set; }

	public ViewInterpolationHelper(AbstractUnitEntityView view)
	{
		m_View = view;
	}

	public void Interpolate(float progress)
	{
		AbstractUnitEntity entityData = m_View.EntityData;
		if (entityData.Movable.PreviousSimulationTick.HasMotion || m_ForceUpdatePosition)
		{
			m_View.transform.position = Vector3.LerpUnclamped(m_InterpolationFrom, m_InterpolationTo, progress);
		}
		if (m_View.HasOverriddenRotatablePart)
		{
			TurretInterpolatedOrientation = (entityData.Movable.PreviousSimulationTick.HasRotation ? Mathf.LerpAngle(m_PreviousOrientation, entityData.Orientation, progress) : entityData.Orientation);
		}
		else if (entityData.Movable.PreviousSimulationTick.HasRotation && !m_View.ForbidRotation)
		{
			float y = Mathf.LerpAngle(m_PreviousOrientation, entityData.Orientation, progress);
			m_View.transform.rotation = Quaternion.Euler(0f, y, 0f);
		}
	}

	public void OnUnitSimulationTickCompleted(bool forceUpdatePositions)
	{
		m_ForceUpdatePosition = forceUpdatePositions;
		m_InterpolationFrom = m_InterpolationTo;
		m_InterpolationTo = GetViewPosition(m_View.EntityData.Position);
		m_PreviousOrientation = m_View.EntityData.Movable.PreviousOrientation;
	}

	public void ForceUpdatePosition(Vector3 position, float orientation)
	{
		m_InterpolationFrom = position;
		m_InterpolationTo = position;
		m_PreviousOrientation = orientation;
	}

	public void ApplyPlatformDelta()
	{
		AbstractUnitEntity entityData = m_View.EntityData;
		EntityPartStayOnPlatform entityPartStayOnPlatform = entityData?.GetOptional<EntityPartStayOnPlatform>();
		if (entityPartStayOnPlatform != null && entityPartStayOnPlatform.IsOnPlatform())
		{
			Vector3 platformDeltaSinceLastUpdate = entityPartStayOnPlatform.GetPlatformDeltaSinceLastUpdate();
			Vector3 vector = GetViewPosition(entityData.Position) + platformDeltaSinceLastUpdate;
			m_View.transform.position = vector;
			m_InterpolationTo = vector;
		}
	}

	public Vector3 GetViewPosition(Vector3 mechanicsPosition)
	{
		return m_View.GetViewPosition(mechanicsPosition);
	}
}
