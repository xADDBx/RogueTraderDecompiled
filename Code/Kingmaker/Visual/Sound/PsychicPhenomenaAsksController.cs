using System;
using Kingmaker.Controllers.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Visual.Sound;

public class PsychicPhenomenaAsksController : IUnitAsksController, IDisposable, IGlobalRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, ISubscriber, IGlobalRulebookSubscriber, IFakePsychicPhenomenaTrigger, ISubscriber<IBaseUnitEntity>
{
	public PsychicPhenomenaAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public BarkWrapper GetPhenomenaBark(BaseUnitEntity unit, EffectsState effectsState)
	{
		if (!(unit?.View == null) && unit.View.Asks != null)
		{
			switch (effectsState)
			{
			case EffectsState.None:
				break;
			case EffectsState.PsychicPhenomena:
				return unit.View.Asks.PsychicPhenomena;
			case EffectsState.PerilsOfTheWarp:
				return unit.View.Asks.PerilsOfTheWarp;
			default:
				throw new NotImplementedException();
			}
		}
		return null;
	}

	public void OnEventAboutToTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
		TryTriggerAsk(evt.IsPsychicPhenomena, evt.IsPerilsOfTheWarp);
	}

	public void HandleFakePsychicPhenomena(bool isPsychicPhenomena, bool isPerilsOfTheWarp)
	{
		TryTriggerAsk(isPsychicPhenomena, isPerilsOfTheWarp);
	}

	private void TryTriggerAsk(bool isPsychicPhenomena, bool isPerilsOfTheWarp)
	{
		EffectsState phenomenaState = (isPsychicPhenomena ? EffectsState.PsychicPhenomena : (isPerilsOfTheWarp ? EffectsState.PerilsOfTheWarp : EffectsState.None));
		BaseUnitEntity randomPartyEntity = UnitAsksHelper.GetRandomPartyEntity(delegate(BaseUnitEntity x)
		{
			if (x.LifeState.IsConscious)
			{
				BarkWrapper phenomenaBark = GetPhenomenaBark(x, phenomenaState);
				if (phenomenaBark != null && phenomenaBark.HasBarks)
				{
					return !phenomenaBark.IsOnCooldown;
				}
				return false;
			}
			return false;
		});
		if (randomPartyEntity != null && randomPartyEntity.View.Asks != null)
		{
			GetPhenomenaBark(randomPartyEntity, phenomenaState)?.Schedule();
		}
	}
}
