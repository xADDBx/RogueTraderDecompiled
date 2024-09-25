using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventToggleFxAnimator : AnimationClipEvent
{
	[SerializeField]
	private string m_Name;

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

	public AnimationClipEventToggleFxAnimator()
	{
	}

	public AnimationClipEventToggleFxAnimator(float time)
		: this(time, null)
	{
	}

	public AnimationClipEventToggleFxAnimator(float time, string name)
		: base(time)
	{
		m_Name = name;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().FxAnimatorToggleAction(Name);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventToggleFxAnimator(base.Time, Name);
	}

	public override string ToString()
	{
		return $"{Name} toggle FX event at {base.Time}";
	}
}
