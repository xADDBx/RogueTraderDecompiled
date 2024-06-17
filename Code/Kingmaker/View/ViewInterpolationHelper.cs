using Kingmaker.Code.Enums.Helper;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.View;

public class ViewInterpolationHelper
{
	private readonly AbstractUnitEntityView m_View;

	private bool m_ForceUpdatePosition;

	private Vector3 m_NextInterpolationPosition;

	private Vector3 m_PreviousInterpolationPosition;

	private bool m_ShouldRecalculateNextInterpolationPosition;

	private float m_PreviousOrientation;

	public ViewInterpolationHelper(AbstractUnitEntityView view)
	{
		m_View = view;
	}

	public void Interpolate(float progress)
	{
		AbstractUnitEntity entityData = m_View.EntityData;
		if (m_ShouldRecalculateNextInterpolationPosition)
		{
			m_ShouldRecalculateNextInterpolationPosition = false;
			m_NextInterpolationPosition = GetViewPosition(entityData.Position);
		}
		if (entityData.Movable.PreviousSimulationTick.HasMotion || m_ForceUpdatePosition)
		{
			Vector3 position = Vector3.LerpUnclamped(m_PreviousInterpolationPosition, m_NextInterpolationPosition, progress);
			m_View.transform.position = position;
		}
		if (entityData.Movable.PreviousSimulationTick.HasRotation && !m_View.ForbidRotation)
		{
			float y = Mathf.LerpAngle(m_PreviousOrientation, entityData.Orientation, progress);
			if (!m_View.OverrideRotatablePart)
			{
				m_View.transform.rotation = Quaternion.Euler(0f, y, 0f);
			}
			else
			{
				m_View.OverrideRotatablePart.transform.rotation = Quaternion.Euler(0f, y, 0f);
			}
		}
	}

	public void OnUnitSimulationTickCompleted(bool forceUpdatePositions)
	{
		m_ShouldRecalculateNextInterpolationPosition = true;
		m_ForceUpdatePosition = forceUpdatePositions;
		m_PreviousInterpolationPosition = m_NextInterpolationPosition;
		m_PreviousOrientation = m_View.EntityData.Movable.PreviousOrientation;
	}

	public void ForceUpdatePosition(Vector3 position, float orientation)
	{
		m_PreviousInterpolationPosition = position;
		m_NextInterpolationPosition = position;
		m_PreviousOrientation = orientation;
	}

	public Vector3 GetViewPosition(Vector3 mechanicsPosition)
	{
		if (m_View.MovementAgent.NodeLinkTraverser.IsTraverseNow)
		{
			return SizePathfindingHelper.FromMechanicsToViewPosition(m_View.EntityData, mechanicsPosition);
		}
		return m_View.GetViewPositionOnGround(mechanicsPosition);
	}
}
