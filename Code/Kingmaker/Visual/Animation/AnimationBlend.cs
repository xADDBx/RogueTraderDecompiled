using System;
using System.Collections.Generic;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimationBlend : AnimationBase, IAddableAnimation
{
	private PlayableInfo m_PlayableInfo1;

	private PlayableInfo m_PlayableInfo2;

	private float m_Blend = 1f;

	private float m_Weight;

	public IEnumerable<PlayableInfo> PlayableInfos
	{
		get
		{
			if (m_PlayableInfo1 != null)
			{
				yield return m_PlayableInfo1;
			}
			if (m_PlayableInfo2 != null)
			{
				yield return m_PlayableInfo2;
			}
		}
	}

	public float Blend
	{
		get
		{
			return m_Blend;
		}
		set
		{
			m_Blend = value;
			SetWeight(m_Weight);
		}
	}

	public AnimationBlend(AnimationActionHandle handle)
		: base(handle)
	{
	}

	public void AddPlayableInfo(PlayableInfo playableInfo)
	{
		if (m_PlayableInfo1 == null)
		{
			m_PlayableInfo1 = playableInfo;
			m_LastSetSpeed = playableInfo.GetSpeed();
			return;
		}
		if (m_PlayableInfo2 == null)
		{
			m_PlayableInfo2 = playableInfo;
			return;
		}
		throw new InvalidOperationException("Can only blend two clips with AnimationBlend");
	}

	public override void UpdateEvents()
	{
		m_PlayableInfo1?.UpdateEvents();
	}

	public override void StopEvents()
	{
		m_PlayableInfo1?.StopEvents();
	}

	public override void StopEvents(IEnumerable<AnimationClipEvent> events)
	{
		m_PlayableInfo1?.StopEvents(events);
	}

	public override AnimationClip GetPlayableClip()
	{
		return m_PlayableInfo1?.GetPlayableClip();
	}

	public override RuntimeAnimatorController GetPlayableController()
	{
		return m_PlayableInfo1?.GetPlayableController();
	}

	public override AnimationClip GetActiveClip()
	{
		return m_PlayableInfo1?.GetActiveClip();
	}

	public override void SetWeight(float weight)
	{
		m_Weight = weight;
		m_PlayableInfo1?.SetWeight(weight * m_Blend);
		m_PlayableInfo2?.SetWeight(weight * (1f - m_Blend));
	}

	public override void SetWeightMultiplier(float weight)
	{
		m_PlayableInfo1?.SetWeightMultiplier(weight);
		m_PlayableInfo2?.SetWeightMultiplier(weight);
	}

	public override float GetWeightMultiplier()
	{
		return m_PlayableInfo1?.GetWeightMultiplier() ?? m_PlayableInfo2?.GetWeightMultiplier() ?? 0f;
	}

	public override float GetWeight()
	{
		return m_Weight;
	}

	public override void SetSpeed(float speed)
	{
		if (NeedChangeSpeed(speed))
		{
			m_LastSetSpeed = speed;
			m_PlayableInfo1?.SetSpeed(speed);
			m_PlayableInfo2?.SetSpeed(speed);
		}
	}

	public override float GetSpeed()
	{
		if (m_PlayableInfo1 == null)
		{
			return 0f;
		}
		return m_LastSetSpeed;
	}

	public override float GetTime()
	{
		return m_PlayableInfo1?.GetTime() ?? 0f;
	}

	public override void SetTime(float time)
	{
		if (NeedChangeTime(time))
		{
			m_LastSetTime = time;
			m_PlayableInfo1?.SetTime(time);
			m_PlayableInfo2?.SetTime(time);
		}
	}

	public override void RemoveFromManager()
	{
		m_PlayableInfo1?.RemoveFromManager();
		m_PlayableInfo2?.RemoveFromManager();
	}

	public override AnimationBase Find(AvatarMask mask, bool isAdditive)
	{
		if (m_PlayableInfo1?.MixerInfo.AvatarMask == mask)
		{
			PlayableInfo playableInfo = m_PlayableInfo1;
			if (playableInfo != null && playableInfo.MixerInfo.IsAdditive == isAdditive)
			{
				return this;
			}
		}
		return null;
	}
}
