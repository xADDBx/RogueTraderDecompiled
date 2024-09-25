using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation;

[Serializable]
public class CutsceneAnimationClip
{
	private AnimationClipPlayable m_Playable;

	private bool m_IsInitialized;

	[SerializeField]
	private AnimationClip m_AnimationClip;

	[SerializeField]
	private float m_TransitionIn;

	[SerializeField]
	private float m_TransitionOut;

	[SerializeField]
	private bool m_IsConsistent;

	[SerializeField]
	private bool m_IsEndless;

	internal float ForceTimeOut = -1f;

	internal bool ConsistentCheck;

	public AnimationClipPlayable Playable
	{
		get
		{
			if (!m_IsInitialized)
			{
				m_IsInitialized = true;
			}
			return m_Playable;
		}
	}

	public AnimationClip Clip => m_AnimationClip;

	public float TransitionIn => m_TransitionIn;

	public float TransitionOut => m_TransitionOut;

	public bool IsConsistent => m_IsConsistent;

	public CutsceneAnimationClip(AnimationClip clip, float transitionIn, float transitionOut, bool isConsisten, bool isEndless)
	{
		m_AnimationClip = clip;
		m_TransitionIn = transitionIn;
		m_TransitionOut = transitionOut;
		m_IsConsistent = isConsisten;
		m_IsEndless = isEndless;
	}

	public float CalculateWeightByTime()
	{
		float result = 1f;
		double time = m_Playable.GetTime();
		double duration = m_Playable.GetDuration();
		m_TransitionIn = Mathf.Clamp(m_TransitionIn, 0f, (float)duration);
		m_TransitionOut = Mathf.Clamp(m_TransitionOut, 0f, (float)duration);
		if (ForceTimeOut >= 0f)
		{
			result = ((!(m_TransitionOut <= 0f)) ? (1f - Mathf.Clamp01(ForceTimeOut / m_TransitionOut)) : 0f);
		}
		else if (time > duration - (double)m_TransitionOut)
		{
			result = ((m_IsEndless && !ConsistentCheck) ? 1f : ((!(m_TransitionOut <= 0f)) ? (1f - Mathf.Clamp01((float)((time - (duration - (double)m_TransitionOut)) / (double)m_TransitionOut))) : 0f));
		}
		else if (time < (double)m_TransitionIn)
		{
			result = ((!(m_TransitionIn <= 0f)) ? Mathf.Clamp((float)(time / (double)m_TransitionIn), 0.01f, 1f) : 1f);
		}
		return result;
	}

	public bool IsTransitionOut()
	{
		double time = m_Playable.GetTime();
		double duration = m_Playable.GetDuration();
		return time > duration - (double)TransitionOut;
	}
}
