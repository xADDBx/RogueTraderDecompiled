using System.Collections.Generic;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Events;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimationComposition : AnimationBase, IAddableAnimation
{
	private readonly List<PlayableInfo> m_PlayableInfos = new List<PlayableInfo>();

	public IEnumerable<PlayableInfo> PlayableInfos => m_PlayableInfos;

	public AnimationComposition(AnimationActionHandle handle)
		: base(handle)
	{
	}

	public void AddPlayableInfo(PlayableInfo playableInfo)
	{
		m_PlayableInfos.Add(playableInfo);
		if (m_PlayableInfos.Count == 1)
		{
			m_LastSetSpeed = playableInfo.GetSpeed();
		}
	}

	public override void UpdateEvents()
	{
		if (m_PlayableInfos.Count > 0)
		{
			m_PlayableInfos[0].UpdateEvents();
		}
	}

	public override void StopEvents()
	{
		if (m_PlayableInfos.Count > 0)
		{
			m_PlayableInfos[0].StopEvents();
		}
	}

	public override void StopEvents(IEnumerable<AnimationClipEvent> events)
	{
		if (m_PlayableInfos.Count > 0)
		{
			m_PlayableInfos[0].StopEvents(events);
		}
	}

	public override AnimationClip GetPlayableClip()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_PlayableInfos[0].GetPlayableClip();
		}
		return null;
	}

	public override RuntimeAnimatorController GetPlayableController()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_PlayableInfos[0].GetPlayableController();
		}
		return null;
	}

	public override AnimationClip GetActiveClip()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_PlayableInfos[0].GetActiveClip();
		}
		return null;
	}

	public override void SetWeight(float weight)
	{
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			m_PlayableInfos[i].SetWeight(weight);
		}
	}

	public override void SetWeightMultiplier(float weight)
	{
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			m_PlayableInfos[i].SetWeightMultiplier(weight);
		}
	}

	public override float GetWeight()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_PlayableInfos[0].GetWeight();
		}
		return 0f;
	}

	public override float GetWeightMultiplier()
	{
		if (m_PlayableInfos.Count <= 0)
		{
			return 0f;
		}
		return m_PlayableInfos[0].GetWeightMultiplier();
	}

	public override void SetSpeed(float speed)
	{
		if (NeedChangeSpeed(speed))
		{
			m_LastSetSpeed = speed;
			for (int i = 0; i < m_PlayableInfos.Count; i++)
			{
				m_PlayableInfos[i].SetSpeed(speed);
			}
		}
	}

	public override float GetSpeed()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_LastSetSpeed;
		}
		return 0f;
	}

	public override float GetTime()
	{
		if (m_PlayableInfos.Count > 0)
		{
			return m_PlayableInfos[0].GetTime();
		}
		return 0f;
	}

	public override void SetTime(float time)
	{
		if (NeedChangeTime(time))
		{
			m_LastSetTime = time;
			for (int i = 0; i < m_PlayableInfos.Count; i++)
			{
				m_PlayableInfos[i].SetTime(time);
			}
		}
	}

	public override void RemoveFromManager()
	{
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			m_PlayableInfos[i].RemoveFromManager();
		}
	}

	public override AnimationBase Find(AvatarMask mask, bool isAdditive)
	{
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			AnimationBase animationBase = m_PlayableInfos[i].Find(mask, isAdditive);
			if (animationBase != null)
			{
				return animationBase;
			}
		}
		return null;
	}

	public override void IncrementTime(float deltaTime)
	{
		base.IncrementTime(deltaTime);
		for (int i = 0; i < m_PlayableInfos.Count; i++)
		{
			m_PlayableInfos[i].IncrementTime(deltaTime);
		}
	}
}
