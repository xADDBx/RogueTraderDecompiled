using System;
using Kingmaker.Controllers;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Actions;

public class AnimationActionHandle
{
	private readonly bool m_IsUnitAnimationActionCover;

	private float m_Time;

	private float m_NextTime;

	private float m_SpeedScale = 1f;

	private int m_StartedAtTick;

	private bool m_IsAdditive;

	private int m_ActEventsCounter;

	public AnimationManager Manager { get; set; }

	public AnimationActionBase Action { get; private set; }

	public AnimationBase ActiveAnimation { get; internal set; }

	public bool IsStarted { get; private set; }

	public bool IsAdditive
	{
		get
		{
			if (!m_IsUnitAnimationActionCover)
			{
				return Action.IsAdditive;
			}
			return m_IsAdditive;
		}
		set
		{
			m_IsAdditive = value;
		}
	}

	public bool IsFinished { get; private set; }

	public bool IsSkipped { get; set; }

	public bool IsInterrupted { get; private set; }

	public bool PreventsRotation { get; set; }

	public bool HasCrossfadePriority { get; set; }

	public bool SkipFirstTick { get; set; } = true;


	public bool SkipFirstTickOnHandle { get; set; } = true;


	public bool CorrectTransitionOutTime { get; set; }

	public bool DontReleaseOnInterrupt => Action.DontReleaseOnInterrupt;

	public bool IsReleased { get; private set; }

	public int ActEventsCounter
	{
		get
		{
			return m_ActEventsCounter;
		}
		set
		{
			m_ActEventsCounter = value;
		}
	}

	public bool IsActed => ActEventsCounter > 0;

	public float SpeedScale
	{
		get
		{
			return m_SpeedScale;
		}
		set
		{
			m_SpeedScale = value;
			if (ActiveAnimation != null && !Mathf.Approximately(ActiveAnimation.GetSpeed(), m_SpeedScale))
			{
				ActiveAnimation.SetSpeed(m_SpeedScale);
			}
		}
	}

	internal AnimationActionHandle(AnimationActionBase action, AnimationManager manager)
	{
		Manager = manager;
		Action = action;
		m_IsUnitAnimationActionCover = Action is UnitAnimationActionCover;
	}

	internal void StartInternal()
	{
		try
		{
			Action.OnStart(this);
		}
		catch (Exception ex)
		{
			PFLog.Animations.Exception(ex);
			IsFinished = true;
		}
		IsStarted = true;
		m_StartedAtTick = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		m_Time = 0f;
		m_NextTime = m_Time + RealTimeController.SystemStepDurationSeconds * SpeedScale;
	}

	internal void FinishInternal()
	{
		IsFinished = true;
		try
		{
			Action.OnFinish(this);
		}
		catch (Exception ex)
		{
			PFLog.Animations.Exception(ex);
		}
	}

	internal void UpdateInternal(float deltaTime)
	{
		if (SkipFirstTickOnHandle || Game.Instance.RealTimeController.CurrentSystemStepIndex != m_StartedAtTick)
		{
			m_Time = m_NextTime;
			m_NextTime += deltaTime * SpeedScale;
		}
		try
		{
			Action.OnUpdate(this, deltaTime);
		}
		catch (Exception ex)
		{
			PFLog.Animations.Exception(ex);
			IsFinished = true;
		}
	}

	public float GetTime()
	{
		return m_Time;
	}

	public void StartClip(AnimationClipWrapper clipWrapper, ClipDurationType duration = ClipDurationType.Default)
	{
		bool isAdditive = IsAdditive;
		Manager.AddAnimationClip(this, clipWrapper, Action.AvatarMasks, Action.UseEmptyAvatarMask, isAdditive, duration);
		ActiveAnimation?.SetSpeed(m_SpeedScale);
	}

	public void StartController(RuntimeAnimatorController animatorController)
	{
		bool isAdditive = IsAdditive;
		Manager.AddAnimatorController(this, animatorController, Action.AvatarMasks, Action.UseEmptyAvatarMask, isAdditive);
		ActiveAnimation?.SetSpeed(m_SpeedScale);
	}

	public void Release()
	{
		if (!IsReleased)
		{
			if (ActiveAnimation != null && ActiveAnimation.State != AnimationState.TransitioningOut && ActiveAnimation.State != AnimationState.Finished)
			{
				ActiveAnimation.StartTransitionOut();
				ActiveAnimation.StopEvents();
			}
			IsReleased = true;
		}
	}

	public void ChangeManager(AnimationManager manager)
	{
		Manager = manager;
		Action = ((UnitAnimationManager)Manager).GetAction(((UnitAnimationAction)Action).Type);
	}

	public void Release(float transisionOut)
	{
		Release();
		if (ActiveAnimation != null)
		{
			ActiveAnimation.TransitionOut = transisionOut;
		}
	}

	internal void MarkInterrupted()
	{
		if (IsStarted)
		{
			IsInterrupted = true;
			return;
		}
		bool flag2 = (IsFinished = true);
		bool isInterrupted = (IsReleased = flag2);
		IsInterrupted = isInterrupted;
		Action.OnSequencedInterrupted(this);
	}
}
