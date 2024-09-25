using System;
using Kingmaker.View.Animation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventDecoratorObject : AnimationClipEvent
{
	[SerializeField]
	private UnitAnimationDecoratorObject m_DecoratorObject;

	public UnitAnimationDecoratorObject DecoratorObject
	{
		get
		{
			return m_DecoratorObject;
		}
		set
		{
			m_DecoratorObject = value;
		}
	}

	public AnimationClipEventDecoratorObject()
	{
	}

	public AnimationClipEventDecoratorObject(float time)
		: this(time, null)
	{
	}

	public AnimationClipEventDecoratorObject(float time, UnitAnimationDecoratorObject decoratorObject)
		: base(time)
	{
		m_DecoratorObject = decoratorObject;
	}

	public override Action Start(AnimationManager animationManager)
	{
		animationManager.GetComponent<UnitAnimationCallbackReceiver>().PostDecoratorObject(DecoratorObject);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventDecoratorObject(base.Time, DecoratorObject);
	}

	public override string ToString()
	{
		return $"{DecoratorObject.name} decorator object event at {base.Time}";
	}
}
