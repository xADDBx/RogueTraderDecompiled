using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityDistanceFactor : RulebookTargetEvent
{
	public AbilityData Ability { get; }

	public int BonusEffectiveRange { get; set; }

	public float Result { get; private set; }

	public RuleCalculateAbilityDistanceFactor([NotNull] IMechanicEntity initiator, MechanicEntity target, AbilityData ability)
		: this((MechanicEntity)initiator, target, ability)
	{
	}

	public RuleCalculateAbilityDistanceFactor([NotNull] MechanicEntity initiator, MechanicEntity target, AbilityData ability)
		: base(initiator, target)
	{
		Ability = ability;
		BonusEffectiveRange = 0;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int num = Ability.GetWeaponStats().ResultMaxDistance + BonusEffectiveRange;
		int num2;
		if (context.Previous is RuleCalculateHitChances ruleCalculateHitChances)
		{
			MechanicEntity concreteInitiator = base.ConcreteInitiator;
			MechanicEntity concreteTarget = base.ConcreteTarget;
			num2 = WarhammerGeometryUtils.DistanceToInCells(ruleCalculateHitChances.EffectiveCasterPosition, concreteInitiator.SizeRect, concreteInitiator.Forward, concreteTarget.Position, concreteTarget.SizeRect, concreteTarget.Forward);
		}
		else
		{
			num2 = base.ConcreteInitiator.DistanceToInCells(base.ConcreteTarget);
		}
		Result = ((num2 <= num / 2) ? 1f : ((num2 <= num) ? 0.5f : 0f));
	}
}
