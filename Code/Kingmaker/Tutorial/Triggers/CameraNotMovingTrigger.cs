using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e91c3f986684401aaeecfd1383a83265")]
public class CameraNotMovingTrigger : TutorialTriggerTimer, IGameTimeChangedHandler, ISubscriber, ICameraMovementHandler, IHashable
{
	[SerializeField]
	private float m_CameraAngleDelta = 5f;

	[SerializeField]
	private float m_CameraDistanceDelta = 5f;

	private TimeSpan m_TimeSinceNotMoving = TimeSpan.Zero;

	private Vector3 m_CameraPrevTransform;

	private float m_CameraPrevRotationY;

	private float m_MovedBy;

	private float m_RotatedBy;

	private const float Delta = 1E-06f;

	private CameraRig m_Camera => CameraRig.Instance;

	private float X => Mathf.Abs(m_Camera.TargetPosition.x - m_CameraPrevTransform.x);

	private float Y => Mathf.Abs(m_Camera.TargetPosition.y - m_CameraPrevTransform.y);

	private float Z => Mathf.Abs(m_Camera.TargetPosition.z - m_CameraPrevTransform.z);

	private float Rotation => Mathf.Abs(m_Camera.transform.rotation.y - m_CameraPrevRotationY);

	public void HandleGameTimeChanged(TimeSpan delta)
	{
		if (CanStart && !IsDone && !(Game.Instance.CurrentMode != GameModeType.Default))
		{
			if (X <= 1E-06f && Y <= 1E-06f && Z <= 1E-06f && Rotation <= 1E-06f)
			{
				m_CameraPrevTransform.x = m_Camera.TargetPosition.x;
				m_CameraPrevTransform.y = m_Camera.TargetPosition.y;
				m_CameraPrevTransform.z = m_Camera.TargetPosition.z;
				m_CameraPrevRotationY = m_Camera.transform.rotation.y;
				m_TimeSinceNotMoving += delta;
			}
			if (m_MovedBy >= m_CameraDistanceDelta && m_RotatedBy >= m_CameraAngleDelta)
			{
				IsDone = true;
			}
			if (m_TimeSinceNotMoving.Seconds >= TimerValue && !IsDone)
			{
				Actions.Run();
				IsDone = true;
			}
		}
	}

	public void HandleCameraRotated(float angle)
	{
		if (!IsDone)
		{
			m_RotatedBy += angle;
		}
	}

	public void HandleCameraTransformed(float distance)
	{
		if (!IsDone)
		{
			m_MovedBy += distance;
		}
	}

	public override void HandleTimerStart()
	{
		base.HandleTimerStart();
		m_CameraPrevTransform = m_Camera.TargetPosition;
		m_CameraPrevRotationY = m_Camera.transform.rotation.y;
	}

	public void HandleNonGameTimeChanged()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
