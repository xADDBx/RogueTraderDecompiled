using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

public class PlayableInfo : AnimationBase
{
	public readonly Playable Playable;

	public readonly MixerInfo MixerInfo;

	public readonly int InputIndex;

	private List<AnimatorClipInfo> s_TempClipInfos = new List<AnimatorClipInfo>();

	private AnimationClipEvent[] m_Events;

	private Dictionary<AnimationClipEvent, Action> m_LoopedEventToStopAction = new Dictionary<AnimationClipEvent, Action>();

	private int m_NextEventIndex;

	private float m_LastTime;

	private float m_WeightMultiplier = 1f;

	private float m_Weight;

	private int m_LoopCount;

	private bool m_IsFirstUpdate;

	private bool m_IsSuspended;

	private AnimationClip m_Clip;

	private bool m_IsStopped;

	public bool IsSuspended => m_IsSuspended;

	public bool IsStopped => m_IsStopped;

	public int NextEventIndex => m_NextEventIndex;

	public IReadOnlyList<AnimationClipEvent> Events => m_Events;

	internal PlayableInfo(AnimationActionHandle handle, Playable playable, IEnumerable<AnimationClipEvent> events, MixerInfo mixer, int inputIndex)
		: base(handle)
	{
		Playable = playable;
		MixerInfo = mixer;
		InputIndex = inputIndex;
		m_Events = events?.OrderBy((AnimationClipEvent _event) => _event.Time).ToArray();
		if (Playable.IsPlayableOfType<AnimationClipPlayable>())
		{
			m_Clip = ((AnimationClipPlayable)Playable).GetAnimationClip();
		}
		m_LastSetSpeed = (float)playable.GetSpeed();
		Reset(handle);
	}

	internal void Reset(AnimationActionHandle handle)
	{
		StopEvents();
		base.Handle = handle;
		base.CreationTime = ((handle.Manager.PlayableGraph.GetTimeUpdateMode() == DirectorUpdateMode.UnscaledGameTime) ? Game.Instance.TimeController.RealTime : Game.Instance.TimeController.GameTime);
		Playable.SetTime(0.0);
		Playable.SetDone(value: false);
		Playable.Play();
		base.State = AnimationState.TransitioningIn;
		m_NextEventIndex = 0;
		m_LastTime = 0f;
		base.Time = 0f;
		m_WeightMultiplier = 1f;
		m_Weight = 0f;
		m_LoopCount = 0;
		m_IsFirstUpdate = true;
		m_IsSuspended = false;
		m_IsStopped = false;
	}

	public bool CanBeUsedFromCache(AnimationClip clip, IReadOnlyList<AnimationClipEvent> eventsSorted)
	{
		if (GetPlayableClip() != clip)
		{
			return false;
		}
		if (m_Events.Length != eventsSorted.Count)
		{
			return false;
		}
		for (int i = 0; i < m_Events.Length; i++)
		{
			if (m_Events[i] != eventsSorted[i])
			{
				return false;
			}
		}
		return true;
	}

	public override void StopEvents(IEnumerable<AnimationClipEvent> events)
	{
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			AnimationClipEvent @event = item.Key;
			Action value = item.Value;
			if (events.FirstOrDefault((AnimationClipEvent _event) => _event.DoNotStartIfStarted(@event)) == null)
			{
				value();
			}
		}
		m_LoopedEventToStopAction.Clear();
		m_IsStopped = true;
	}

	public override void StopEvents()
	{
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			item.Value?.Invoke();
		}
		m_LoopedEventToStopAction.Clear();
		m_IsStopped = true;
	}

	public void SuspendEvents()
	{
		if (m_IsSuspended)
		{
			return;
		}
		foreach (AnimationClipEvent item in m_LoopedEventToStopAction.Keys.ToTempList())
		{
			m_LoopedEventToStopAction[item]?.Invoke();
			m_LoopedEventToStopAction[item] = null;
		}
		m_IsSuspended = true;
	}

	public void ResumeEvents()
	{
		if (!m_IsSuspended)
		{
			return;
		}
		foreach (AnimationClipEvent item in m_LoopedEventToStopAction.Keys.ToTempList())
		{
			m_LoopedEventToStopAction[item] = item.Start(base.Handle.Manager);
		}
		m_IsSuspended = false;
	}

	public void StartEvents(float time)
	{
		m_IsStopped = false;
		SkipEvents(time);
	}

	public void StartEvents(IEnumerable<AnimationClipEvent> events, float time)
	{
		Dictionary<AnimationClipEvent, Action> dictionary = null;
		foreach (KeyValuePair<AnimationClipEvent, Action> item in m_LoopedEventToStopAction)
		{
			bool flag = false;
			foreach (AnimationClipEvent @event in events)
			{
				if (@event.DoNotStartIfStarted(item.Key) && item.Value != null)
				{
					dictionary = dictionary ?? new Dictionary<AnimationClipEvent, Action>();
					dictionary.Add(@event, item.Value);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				item.Value?.Invoke();
			}
		}
		if (dictionary == null)
		{
			m_LoopedEventToStopAction.Clear();
		}
		else
		{
			m_LoopedEventToStopAction = dictionary;
		}
		m_Events = events?.ToArray();
		if (m_Events != null)
		{
			Array.Sort(m_Events, (AnimationClipEvent evt1, AnimationClipEvent evt2) => evt1.Time.CompareTo(evt2.Time));
		}
		m_LastTime = time;
		m_NextEventIndex = 0;
		m_LoopCount = 0;
		m_IsFirstUpdate = true;
		m_IsStopped = false;
		while (m_NextEventIndex < m_Events.Length && m_Events[m_NextEventIndex].Time < m_LastTime)
		{
			m_NextEventIndex++;
		}
	}

	public override void UpdateEvents()
	{
		if (m_Events == null || m_Events.Length == 0 || m_IsStopped || Playable.IsPlayableOfType<AnimatorControllerPlayable>())
		{
			return;
		}
		AnimationClip playableClip = GetPlayableClip();
		if (playableClip == null)
		{
			PFLog.Animations.Warning("PlayableInfo has no AnimatorController and no playable clip.");
			return;
		}
		float num = base.Time;
		if (!playableClip.isLooping && num > playableClip.length)
		{
			if (!(Game.CombatAnimSpeedUp > 1f) || !m_IsFirstUpdate)
			{
				StopEvents();
				return;
			}
			num = playableClip.length * 0.99f;
		}
		if (!(playableClip.length < 0.1f))
		{
			UpdateEvents(num % playableClip.length, playableClip.length);
		}
	}

	public void SkipEvents(float time)
	{
		if (time < m_LastTime)
		{
			m_NextEventIndex = 0;
			m_LastTime = 0f;
			m_LoopCount++;
		}
		m_LastTime = time;
		while (m_NextEventIndex < m_Events.Length && m_Events[m_NextEventIndex].Time < time)
		{
			m_NextEventIndex++;
		}
	}

	public void UpdateEvents(float time, float length)
	{
		if (m_Events == null || m_Events.Length == 0 || m_IsStopped)
		{
			return;
		}
		if (GetAdjustedWeight() < 0.01f && !m_IsFirstUpdate)
		{
			AnimationClip playableClip = GetPlayableClip();
			if (playableClip == null || playableClip.isLooping)
			{
				SkipEvents(time);
			}
			SuspendEvents();
			return;
		}
		float num = 0.5f;
		if (Mathf.Approximately(time, m_LastTime))
		{
			if (!m_IsFirstUpdate)
			{
				return;
			}
		}
		else if (time > m_LastTime)
		{
			if (time - m_LastTime > num)
			{
				SkipEvents(time - num);
			}
		}
		else if (time + length - m_LastTime > num)
		{
			SkipEvents((time + length - num) % length);
		}
		if (m_IsSuspended)
		{
			ResumeEvents();
		}
		if (time < m_LastTime)
		{
			while (m_NextEventIndex < m_Events.Length)
			{
				try
				{
					StartEvent(m_Events[m_NextEventIndex++]);
				}
				catch (Exception ex)
				{
					PFLog.Animations.Exception(ex);
				}
			}
			m_NextEventIndex = 0;
			m_LastTime = 0f;
			m_LoopCount++;
		}
		float num2 = (m_IsFirstUpdate ? (RealTimeController.SystemStepDurationSeconds * GetSpeed()) : (time - m_LastTime));
		float num3 = time + num2 / 2f;
		m_LastTime = time;
		while (m_NextEventIndex < m_Events.Length && m_Events[m_NextEventIndex].Time < num3)
		{
			try
			{
				StartEvent(m_Events[m_NextEventIndex++]);
			}
			catch (Exception ex2)
			{
				PFLog.Animations.Exception(ex2);
			}
		}
		m_IsFirstUpdate = false;
	}

	private void StartEvent(AnimationClipEvent @event)
	{
		if (!@event.IsLooped || !m_LoopedEventToStopAction.ContainsKey(@event))
		{
			Action action = @event.Start(base.Handle.Manager);
			if (action != null)
			{
				m_LoopedEventToStopAction.Add(@event, action);
			}
		}
	}

	public sealed override AnimationClip GetPlayableClip()
	{
		if (Playable.IsPlayableOfType<AnimationClipPlayable>())
		{
			return ((AnimationClipPlayable)Playable).GetAnimationClip();
		}
		return null;
	}

	public override RuntimeAnimatorController GetPlayableController()
	{
		if (Playable.IsPlayableOfType<AnimatorControllerPlayable>())
		{
			return base.AnimatorController;
		}
		return null;
	}

	public override AnimationClip GetActiveClip()
	{
		if (Playable.IsPlayableOfType<AnimationClipPlayable>())
		{
			return ((AnimationClipPlayable)Playable).GetAnimationClip();
		}
		if (Playable.IsPlayableOfType<AnimatorControllerPlayable>())
		{
			AnimatorControllerPlayable animatorControllerPlayable = (AnimatorControllerPlayable)Playable;
			int layerIndex = 0;
			float num = 0f;
			int layerCount = animatorControllerPlayable.GetLayerCount();
			for (int i = 0; i < layerCount; i++)
			{
				float layerWeight = animatorControllerPlayable.GetLayerWeight(i);
				if (layerWeight > num)
				{
					num = layerWeight;
					layerIndex = i;
				}
			}
			animatorControllerPlayable.GetCurrentAnimatorClipInfo(layerIndex, s_TempClipInfos);
			AnimationClip clip = s_TempClipInfos.ElementAtOrDefault(0).clip;
			s_TempClipInfos.Clear();
			return clip;
		}
		return null;
	}

	public override void SetWeight(float weight)
	{
		m_Weight = weight;
	}

	public override void SetWeightMultiplier(float weight)
	{
		float weight2 = GetWeight();
		m_WeightMultiplier = weight;
		SetWeight(weight2);
	}

	public override float GetWeightMultiplier()
	{
		return m_WeightMultiplier;
	}

	public override float GetWeight()
	{
		return m_Weight;
	}

	public float GetAdjustedWeight()
	{
		return m_Weight * m_WeightMultiplier;
	}

	public void SetAdjustedWeight(float weight)
	{
		if (!(m_WeightMultiplier <= 0.001f))
		{
			m_Weight = weight / m_WeightMultiplier;
		}
	}

	public override void SetSpeed(float speed)
	{
		if (NeedChangeSpeed(speed))
		{
			m_LastSetSpeed = speed;
			Playable.SetSpeed(speed);
		}
	}

	public override float GetSpeed()
	{
		return m_LastSetSpeed;
	}

	public override float GetTime()
	{
		return base.Time;
	}

	public override void SetTime(float time)
	{
		if (NeedChangeTime(time))
		{
			m_LastSetTime = time;
			base.Time = time;
			Playable.SetTime(time);
		}
	}

	public override void RemoveFromManager()
	{
		MixerInfo.RemovePlayable(this);
		if (Playable.IsValid())
		{
			Playable.Pause();
		}
	}

	public override AnimationBase Find(AvatarMask mask, bool isAdditive)
	{
		if (MixerInfo.AvatarMask == mask && MixerInfo.IsAdditive == isAdditive)
		{
			return this;
		}
		return null;
	}
}
