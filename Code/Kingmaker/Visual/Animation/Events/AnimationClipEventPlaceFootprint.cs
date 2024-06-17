using System;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventPlaceFootprint : AnimationClipEvent
{
	[SerializeField]
	private string m_Locator;

	[SerializeField]
	private int m_FootIndex;

	public string Locator
	{
		get
		{
			return m_Locator;
		}
		set
		{
			m_Locator = value;
		}
	}

	public int FootIndex
	{
		get
		{
			return m_FootIndex;
		}
		set
		{
			m_FootIndex = value;
		}
	}

	public AnimationClipEventPlaceFootprint(float time)
		: this(time, null, 0)
	{
	}

	public AnimationClipEventPlaceFootprint(float time, string locator, int footIndex)
		: base(time)
	{
		m_Locator = locator;
		m_FootIndex = footIndex;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().PlaceFootprintEvent(Locator, FootIndex);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventPlaceFootprint(base.Time, Locator, FootIndex);
	}

	public override string ToString()
	{
		return $"Place footprint by index {FootIndex} in unit locator {Locator} at {base.Time}";
	}
}
