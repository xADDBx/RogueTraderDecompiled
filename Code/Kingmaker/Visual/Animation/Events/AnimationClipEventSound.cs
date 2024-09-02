using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventSound : AnimationClipEvent
{
	[SerializeField]
	private string m_Name;

	[SerializeField]
	private string m_StopName;

	[SerializeField]
	private float m_Volume;

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public string StopName
	{
		get
		{
			return m_StopName;
		}
		set
		{
			m_StopName = value;
		}
	}

	public float Volume => m_Volume;

	public AnimationClipEventSound()
	{
	}

	public AnimationClipEventSound(float time)
		: this(time, isLooped: false, null, null, 1f)
	{
	}

	public AnimationClipEventSound(float time, bool isLooped, string name, string stopName, float volume)
		: base(time, isLooped)
	{
		m_Name = name;
		m_StopName = stopName;
		m_Volume = volume;
	}

	public override Action Start(AnimationManager animationManager)
	{
		uint uniqueId = animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostEvent(Name, Volume);
		Action result = ((!string.IsNullOrEmpty(StopName)) ? ((Action)delegate
		{
			animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostEvent(StopName, Volume);
		}) : ((Action)delegate
		{
			animationManager.GetComponent<UnitAnimationCallbackReceiver>().StopPlayingById(uniqueId);
		}));
		if (!base.IsLooped)
		{
			return null;
		}
		return result;
	}

	public override bool DoNotStartIfStarted(AnimationClipEvent @object)
	{
		return (@object as AnimationClipEventSound)?.Name == Name;
	}

	public override object Clone()
	{
		return new AnimationClipEventSound(base.Time, base.IsLooped, Name, StopName, Volume);
	}

	public override string ToString()
	{
		return $"{Name} sound event at {base.Time}";
	}
}
