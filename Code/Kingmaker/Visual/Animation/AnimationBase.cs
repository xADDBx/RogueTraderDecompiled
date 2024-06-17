using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public abstract class AnimationBase
{
	private float m_PreviousTime;

	private float m_NextTime;

	protected float m_LastSetSpeed;

	protected float m_LastSetTime;

	protected float m_OverridedDuration = -1f;

	protected float Time
	{
		get
		{
			return m_PreviousTime;
		}
		set
		{
			m_PreviousTime = value;
			m_NextTime = value;
		}
	}

	public AnimationActionHandle Handle { get; protected set; }

	public TimeSpan CreationTime { get; protected set; }

	public AnimationState State { get; protected set; }

	public RuntimeAnimatorController AnimatorController { get; internal set; }

	public bool DoNotZeroOtherAnimations { get; set; }

	public float TransitionIn { get; internal set; }

	public float TransitionOut { get; internal set; }

	public float TransitionOutStartTime { get; internal set; }

	protected bool NeedChangeSpeed(float speed)
	{
		if (Mathf.Approximately(speed, 0f) || Mathf.Approximately(m_LastSetSpeed, 0f) || Mathf.Approximately(speed, 1f) || Mathf.Approximately(m_LastSetSpeed, 1f))
		{
			return true;
		}
		return Mathf.Abs(speed - m_LastSetSpeed) >= 0.001f;
	}

	protected bool NeedChangeTime(float time)
	{
		if (m_LastSetTime == time)
		{
			return false;
		}
		if (time == 0f || m_LastSetTime == 0f || time == 1f || m_LastSetTime == 1f)
		{
			return true;
		}
		return Mathf.Abs(time - m_LastSetTime) >= 0.001f;
	}

	protected AnimationBase(AnimationActionHandle handle)
	{
		Handle = handle;
	}

	public virtual void IncrementTime(float deltaTime)
	{
		CheckInterpolationTime();
		m_PreviousTime = m_NextTime;
		m_NextTime += deltaTime;
	}

	public abstract void UpdateEvents();

	public abstract void StopEvents();

	public abstract void StopEvents(IEnumerable<AnimationClipEvent> events);

	public abstract AnimationClip GetPlayableClip();

	public abstract RuntimeAnimatorController GetPlayableController();

	public abstract AnimationClip GetActiveClip();

	public abstract void SetWeight(float weight);

	public abstract void SetWeightMultiplier(float weight);

	public abstract float GetWeight();

	public abstract void SetSpeed(float speed);

	public abstract float GetSpeed();

	public abstract float GetTime();

	public abstract void SetTime(float time);

	public abstract void RemoveFromManager();

	public void OverrideDuration(float duration)
	{
		m_OverridedDuration = duration;
		if (m_OverridedDuration > 0f)
		{
			TransitionOutStartTime = m_OverridedDuration - TransitionOut;
		}
		else
		{
			TransitionOutStartTime = 0f;
		}
	}

	public float GetDuration()
	{
		if (m_OverridedDuration >= 0f)
		{
			return m_OverridedDuration;
		}
		AnimationClip playableClip = GetPlayableClip();
		if (playableClip != null && !playableClip.isLooping)
		{
			return playableClip.length;
		}
		return 0f;
	}

	[CanBeNull]
	public abstract AnimationBase Find([CanBeNull] AvatarMask mask, bool isAdditive);

	internal void StartTransitionOut()
	{
		if (State != AnimationState.TransitioningOut && State != AnimationState.Finished)
		{
			TransitionOutStartTime = Time;
			State = AnimationState.TransitioningOut;
			if (this == Handle.ActiveAnimation)
			{
				Handle.Action.OnTransitionOutStarted(Handle);
			}
		}
	}

	public void ChangeTransitionTime(float time)
	{
		if (State != AnimationState.TransitioningOut && State != AnimationState.Finished)
		{
			TransitionOutStartTime = TransitionOutStartTime + TransitionOut - time;
			TransitionOut = time;
		}
	}

	internal void UpdateInternal(float deltaTime, float? weightFromManager = null)
	{
		using (ProfileScope.New("Animation.UpdateInternal"))
		{
			IncrementTime(deltaTime);
			using (ProfileScope.New("UpdateEvents"))
			{
				UpdateEvents();
			}
			switch (State)
			{
			case AnimationState.TransitioningIn:
				using (ProfileScope.New("TransitioningIn"))
				{
					if (!(TransitionIn <= 0f) && !(Time >= TransitionIn))
					{
						break;
					}
					if (TransitionOutStartTime > 0f && Time >= TransitionOutStartTime)
					{
						if (TransitionOut <= 0f)
						{
							StopEvents();
							State = AnimationState.Finished;
						}
						else
						{
							StartTransitionOut();
						}
					}
					else
					{
						State = AnimationState.Playing;
					}
				}
				break;
			case AnimationState.Playing:
				using (ProfileScope.New("Playing"))
				{
					if ((TransitionOutStartTime > 0f && Time >= TransitionOutStartTime) || Handle.IsReleased)
					{
						if (TransitionOut <= 0f)
						{
							StopEvents();
							State = AnimationState.Finished;
							Handle.Release();
						}
						else
						{
							StartTransitionOut();
						}
					}
				}
				break;
			case AnimationState.TransitioningOut:
				using (ProfileScope.New("TransitionOut"))
				{
					if (TransitionOut <= 0f || Time >= TransitionOutStartTime + TransitionOut)
					{
						StopEvents();
						State = AnimationState.Finished;
					}
				}
				break;
			}
			if (weightFromManager.HasValue)
			{
				UpdateWeight(Time, weightFromManager.Value);
			}
		}
	}

	internal void InterpolateInternal(float progress, float weightFromManager, bool force = false)
	{
		using (ProfileScope.New("InterpolateInternal"))
		{
			float num = InterpolateTime(progress);
			if (Mathf.Abs(num - m_PreviousTime) > 0.0001f || force)
			{
				UpdateWeight(num, weightFromManager);
			}
		}
	}

	internal void UpdateWeight(float time, float weightFromManager)
	{
		switch (State)
		{
		case AnimationState.TransitioningIn:
			using (ProfileScope.New("TransitioningIn"))
			{
				float num3 = ((0f < TransitionIn) ? Mathf.Clamp01(time / TransitionIn) : 1f);
				SetWeight(weightFromManager * num3);
				break;
			}
		case AnimationState.Playing:
			using (ProfileScope.New("Playing"))
			{
				SetWeight(weightFromManager);
				break;
			}
		case AnimationState.TransitioningOut:
			using (ProfileScope.New("TransitionOut"))
			{
				float num = time - TransitionOutStartTime;
				float num2 = ((0f < TransitionOut) ? Mathf.Clamp01(num / TransitionOut) : 1f);
				SetWeight(weightFromManager * (1f - num2));
				break;
			}
		}
	}

	private float InterpolateTime(float progress)
	{
		CheckInterpolationTime();
		return Mathf.LerpUnclamped(m_PreviousTime, m_NextTime, progress);
	}

	private void CheckInterpolationTime()
	{
		if (Mathf.Approximately(m_PreviousTime, m_NextTime))
		{
			m_NextTime = m_PreviousTime + RealTimeController.SystemStepDurationSeconds * GetSpeed();
		}
	}
}
