using System;
using Kingmaker.Enums.Sound;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventMapped : AnimationClipEvent
{
	public MappedAnimationEventType Type;

	public AnimationClipEventMapped()
	{
	}

	public AnimationClipEventMapped(float time)
		: base(time)
	{
	}

	public AnimationClipEventMapped(float time, bool isLooped)
		: base(time, isLooped)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		AbstractUnitEntity abstractUnitEntity = (animationManager as UnitAnimationManager)?.View.Or(null)?.EntityData;
		if (abstractUnitEntity != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)abstractUnitEntity, (Action<IAnimationEventHandler>)delegate(IAnimationEventHandler h)
			{
				h.HandleAnimationEvent(Type);
			}, isCheckRuntime: true);
		}
		return null;
	}
}
