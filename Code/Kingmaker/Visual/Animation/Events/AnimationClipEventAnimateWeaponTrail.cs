using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventAnimateWeaponTrail : AnimationClipEvent
{
	[SerializeField]
	private float m_Duration;

	public float Duration
	{
		get
		{
			return m_Duration;
		}
		set
		{
			m_Duration = value;
		}
	}

	public AnimationClipEventAnimateWeaponTrail(float time)
		: this(time, 0f)
	{
	}

	public AnimationClipEventAnimateWeaponTrail(float time, float duration)
		: base(time)
	{
		m_Duration = duration;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().AnimateWeaponTrail(Duration);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventAnimateWeaponTrail(base.Time, Duration);
	}

	public override string ToString()
	{
		return $"Animate weapon tail event at {base.Time}";
	}
}
