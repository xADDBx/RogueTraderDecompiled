using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleFakeCalculatePsychicPhenomenaEffect : RuleCalculatePsychicPhenomenaEffect
{
	public RuleFakeCalculatePsychicPhenomenaEffect([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public RuleFakeCalculatePsychicPhenomenaEffect([NotNull] MechanicEntity initiator, [NotNull] AbilityExecutionContext abilityContext)
		: base(initiator, abilityContext)
	{
	}

	public RuleFakeCalculatePsychicPhenomenaEffect([NotNull] IMechanicEntity initiator, BlueprintAbilityReference resultPerilsEffect, BlueprintPsychicPhenomenaRoot.PsychicPhenomenaData resultPsychicPhenomena)
		: base(initiator)
	{
		base.IsPerilsOfTheWarp = resultPerilsEffect != null;
		base.ResultPerilsEffect = resultPerilsEffect;
		base.IsPsychicPhenomena = resultPsychicPhenomena != null;
		base.ResultPsychicPhenomena = resultPsychicPhenomena;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
	}
}
