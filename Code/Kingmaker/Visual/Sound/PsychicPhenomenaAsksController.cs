using System;
using Kingmaker.Controllers.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Visual.Sound;

public class PsychicPhenomenaAsksController : IUnitAsksController, IDisposable, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public PsychicPhenomenaAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		MechanicEntity caster = context.Caster;
		if (caster is BaseUnitEntity)
		{
			_ = caster.IsPlayerFaction;
		}
	}

	private static BarkWrapper GetPhenomenaBark(BaseUnitEntity unit, EffectsState effectsState)
	{
		if (unit?.View == null || unit.View.Asks == null)
		{
			return null;
		}
		return effectsState switch
		{
			EffectsState.PsychicPhenomena => unit.View.Asks.PsychicPhenomena, 
			EffectsState.PerilsOfTheWarp => unit.View.Asks.PerilsOfTheWarp, 
			_ => throw new NotImplementedException(), 
		};
	}
}
