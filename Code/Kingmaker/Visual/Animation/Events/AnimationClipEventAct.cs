using System;
using Kingmaker.Enums.Sound;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Visual.Animation.Events;

public class AnimationClipEventAct : AnimationClipEvent
{
	public AnimationClipEventAct(float time)
		: base(time)
	{
	}

	public override Action Start(AnimationManager animationManager)
	{
		AbstractUnitEntity abstractUnitEntity = (animationManager as UnitAnimationManager)?.View.Or(null)?.EntityData;
		if (abstractUnitEntity != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)abstractUnitEntity, (Action<IAnimationEventHandler>)delegate(IAnimationEventHandler h)
			{
				h.HandleAnimationEvent(MappedAnimationEventType.Act);
			}, isCheckRuntime: true);
		}
		else if (animationManager.GetComponentInParent<MechadendriteSettings>() != null && animationManager is UnitAnimationManager unitAnimationManager)
		{
			AbstractUnitEntity abstractUnitEntity2 = unitAnimationManager.LocoMotionHandle?.Unit.Data;
			if (abstractUnitEntity2 != null)
			{
				EventBus.RaiseEvent((IMechanicEntity)abstractUnitEntity2, (Action<IAnimationEventHandler>)delegate(IAnimationEventHandler h)
				{
					h.HandleAnimationEvent(MappedAnimationEventType.Act);
				}, isCheckRuntime: true);
				abstractUnitEntity2.MaybeAnimationManager?.GetComponentInParent<UnitAnimationCallbackReceiver>()?.PostCommandActEvent();
				return null;
			}
			return null;
		}
		animationManager.GetComponentInParent<UnitAnimationCallbackReceiver>().PostCommandActEvent();
		return null;
	}

	public override object Clone()
	{
		return new AnimationClipEventAct(base.Time);
	}

	public override string ToString()
	{
		return $"Act event at {base.Time}";
	}
}
