using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Events;

[Serializable]
public class AnimationClipEventExternal : AnimationClipEvent
{
	[SerializeField]
	private BlueprintAnimationActionExternalHandlerReference m_BlueprintAnimationActionExternalHandler;

	[SerializeField]
	private ClipEventType m_ClipEventType;

	[SerializeField]
	private int m_Id;

	public BlueprintAnimationActionExternalHandler BlueprintAnimationActionExternalHandler
	{
		get
		{
			return m_BlueprintAnimationActionExternalHandler?.Get();
		}
		set
		{
			m_BlueprintAnimationActionExternalHandler = value.ToReference<BlueprintAnimationActionExternalHandlerReference>();
		}
	}

	public ClipEventType ClipEventType
	{
		get
		{
			return m_ClipEventType;
		}
		set
		{
			m_ClipEventType = value;
		}
	}

	public int Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
		}
	}

	public AnimationClipEventExternal(float time)
		: base(time)
	{
	}

	public AnimationClipEventExternal(float time, bool isLooped)
		: base(time, isLooped)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		BlueprintAnimationActionExternalHandler.Handle(animationManager, m_ClipEventType, m_Id);
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventExternal(base.Time)
		{
			m_BlueprintAnimationActionExternalHandler = m_BlueprintAnimationActionExternalHandler,
			m_ClipEventType = m_ClipEventType,
			m_Id = m_Id
		};
	}

	public override string ToString()
	{
		return $"External event at {base.Time}";
	}
}
