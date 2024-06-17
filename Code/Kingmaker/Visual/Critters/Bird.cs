using Kingmaker.Controllers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual.Base;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class Bird : MonoBehaviour, IUpdatable, IInterpolatable, IPartyCombatHandler, ISubscriber
{
	public float Speed = 3f;

	private BirdLane m_PreviousLane;

	private BirdLane m_CurrentLane;

	private Vector3 m_PreviousPosition;

	private Vector3 m_CurrentPosition;

	private int m_PreviousTargetPoint;

	private int m_CurrentTargetPoint;

	private float m_PreviousDeltaTime;

	private Vector3 m_PreviousLookAtPosition;

	private int m_LastUpdateTick;

	public BirdLane Lane
	{
		get
		{
			return m_CurrentLane;
		}
		set
		{
			if (value != null && (value.Points == null || value.Points.Length < 2))
			{
				value = null;
			}
			m_PreviousLane = null;
			m_CurrentLane = value;
			m_CurrentTargetPoint = 1;
			if (m_CurrentLane != null)
			{
				Vector3 position = m_CurrentLane.Points[0].position;
				base.transform.position = position;
				m_CurrentPosition = position;
				m_PreviousLookAtPosition = ((1 < m_CurrentLane.Points.Length) ? m_CurrentLane.Points[1].position : Vector3.zero);
				base.gameObject.SetActive(!Game.Instance.Player.IsInCombat);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private static int CurrentUpdateTick => Game.Instance.RealTimeController.CurrentSystemStepIndex;

	public void Init()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		Game.Instance.BirdUpdateController.Add(this);
		Game.Instance.InterpolationController.Add(this);
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		Game.Instance.BirdUpdateController.Remove(this);
		Game.Instance.InterpolationController.Remove(this);
		EventBus.Unsubscribe(this);
	}

	void IUpdatable.Tick(float delta)
	{
		using (ProfileScope.New("Bird.Tick"))
		{
			m_PreviousLane = m_CurrentLane;
			m_PreviousPosition = m_CurrentPosition;
			m_PreviousTargetPoint = m_CurrentTargetPoint;
			m_PreviousDeltaTime = delta;
			if (MoveTowards(delta, m_CurrentLane, ref m_CurrentPosition, ref m_CurrentTargetPoint, out var lookAtPosition))
			{
				Lane = null;
			}
			Transform obj = base.transform;
			obj.position = m_PreviousPosition;
			obj.LookAt(m_PreviousLookAtPosition);
			m_PreviousLookAtPosition = lookAtPosition;
			m_LastUpdateTick = CurrentUpdateTick;
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		using (ProfileScope.New("Bird.Tick"))
		{
			if (!(m_PreviousLane == null) && m_LastUpdateTick == CurrentUpdateTick)
			{
				float delta = Mathf.LerpUnclamped(0f, m_PreviousDeltaTime, progress);
				Vector3 position = m_PreviousPosition;
				int pointIndex = m_PreviousTargetPoint;
				MoveTowards(delta, m_PreviousLane, ref position, ref pointIndex, out var lookAtPosition);
				Transform obj = base.transform;
				obj.position = position;
				obj.LookAt(lookAtPosition);
			}
		}
	}

	private bool MoveTowards(float delta, BirdLane lane, ref Vector3 position, ref int pointIndex, out Vector3 lookAtPosition)
	{
		float distance = Speed * delta;
		bool num = lane.Points.MoveAlong(distance, ref position, ref pointIndex);
		if (num)
		{
			if (1 < lane.Points.Length)
			{
				Vector3 position2 = lane.Points[^1].position;
				Vector3 position3 = lane.Points[^2].position;
				lookAtPosition = position2 + (position2 - position3);
				return num;
			}
			lookAtPosition = Vector3.zero;
			return num;
		}
		lookAtPosition = lane.Points[pointIndex].position;
		return num;
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			Lane = null;
		}
	}
}
