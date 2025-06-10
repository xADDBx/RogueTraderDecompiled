using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public abstract class AnimationBase
{
	private const float EpsilonTime = 1E-06f;

	protected float m_LastSetSpeed = 1f;

	protected float m_LastSetTime;

	protected float m_OverridedDuration = -1f;

	protected float Time { get; set; }

	public bool UpdatedOnce { get; set; }

	public AnimationActionHandle Handle { get; protected set; }

	public TimeSpan CreationTime { get; protected set; }

	public AnimationState State { get; protected set; }

	public RuntimeAnimatorController AnimatorController { get; internal set; }

	public bool DoNotZeroOtherAnimations { get; set; }

	public float TransitionIn { get; internal set; }

	public float TransitionOut { get; internal set; }

	public float TransitionOutStartTime { get; internal set; }

	public float WeightFromManager { get; private set; }

	public bool OnlySendEventsOnce { get; set; }

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

	public virtual void Reset()
	{
		UpdatedOnce = false;
	}

	public virtual void IncrementTime(float deltaTime)
	{
		Time += deltaTime;
		m_LastSetTime = Time;
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

	public abstract float GetWeightMultiplier();

	public abstract void SetSpeed(float speed);

	public abstract float GetSpeed();

	public abstract float GetTime();

	public abstract void SetTime(float time);

	public abstract void RemoveFromManager();

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
		StartTransitionOut(forceSetTime: false);
	}

	private void StartTransitionOut(bool forceSetTime)
	{
		if (State != AnimationState.TransitioningOut && State != AnimationState.Finished)
		{
			TransitionOutStartTime = ((!forceSetTime && Handle.CorrectTransitionOutTime) ? 0f : Time);
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
			if (TransitionOutStartTime > 0f)
			{
				TransitionOutStartTime = Mathf.Max(TransitionOutStartTime + TransitionOut - time, 0.01f);
			}
			TransitionOut = time;
		}
	}

	internal void Update(float deltaTime, float? weightFromManager = null)
	{
		using (ProfileScope.New("Animation.UpdateInternal"))
		{
			if (UpdatedOnce || Handle.SkipFirstTick)
			{
				IncrementTime(deltaTime);
			}
			using (ProfileScope.New("UpdateEvents"))
			{
				UpdateEvents();
			}
			switch (State)
			{
			case AnimationState.TransitioningIn:
				using (ProfileScope.New("TransitioningIn"))
				{
					if (!(TransitionIn <= 0f) && !(Time > TransitionIn - 1E-06f))
					{
						break;
					}
					if (TransitionOutStartTime > 0f && Time > TransitionOutStartTime - 1E-06f)
					{
						if (TransitionOut <= 0f)
						{
							StopEvents();
							State = AnimationState.Finished;
						}
						else
						{
							StartTransitionOut(forceSetTime: true);
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
					if ((TransitionOutStartTime > 0f && Time > TransitionOutStartTime - 1E-06f) || Handle.IsReleased)
					{
						StartTransitionOut(forceSetTime: true);
						if (TransitionOut <= 0f)
						{
							StopEvents();
							State = AnimationState.Finished;
						}
					}
				}
				break;
			case AnimationState.TransitioningOut:
				using (ProfileScope.New("TransitionOut"))
				{
					if (TransitionOutStartTime <= 0f || TransitionOutStartTime > Time)
					{
						TransitionOutStartTime = Time;
					}
					if (TransitionOut <= 0f || Time > TransitionOutStartTime + TransitionOut - 1E-06f)
					{
						StopEvents();
						State = AnimationState.Finished;
					}
				}
				break;
			}
			UpdatedOnce = true;
		}
	}

	internal void Interpolate(float progress, float weightFromManager, bool force = false)
	{
		using (ProfileScope.New("Interpolate"))
		{
			float num = InterpolateTime(progress);
			if (Mathf.Abs(num - Time) > 0.0001f || force)
			{
				UpdateWeight(num, weightFromManager);
			}
		}
	}

	internal void UpdateWeight(float time, float weightFromManager)
	{
		WeightFromManager = weightFromManager;
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
		return Time + Handle.Manager.LastDeltaTime * GetSpeed() * progress;
	}
}
