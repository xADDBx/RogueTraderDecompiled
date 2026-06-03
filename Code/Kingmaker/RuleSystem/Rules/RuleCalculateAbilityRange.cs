using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityRange : RulebookEvent
{
	public class Cache : RuleCache<AbilityData, RuleCalculateAbilityRange>
	{
	}

	public BlueprintAbility SourceAbility;

	public int Result { get; private set; }

	public int DefaultRange { get; }

	public int Bonus { get; set; }

	public int FiringArcBonus { get; set; }

	public AbilityData Ability { get; }

	public int? OverrideRange { get; set; }

	public RuleCalculateAbilityRange([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
		Bonus = 0;
		int range = Ability.Blueprint.GetRange();
		if (range >= 0)
		{
			DefaultRange = range;
		}
		else
		{
			DefaultRange = Ability.GetWeaponStats(initiator).ResultMaxDistance;
		}
	}

	public static RuleCalculateAbilityRange TryGetCachedOrTrigger(AbilityData ability)
	{
		RuleCalculateAbilityRange ruleCalculateAbilityRange = RuleCache<AbilityData, RuleCalculateAbilityRange>.Get(ability);
		if (ruleCalculateAbilityRange == null)
		{
			ruleCalculateAbilityRange = Rulebook.Trigger(new RuleCalculateAbilityRange(ability.Caster, ability));
			RuleCache<AbilityData, RuleCalculateAbilityRange>.Set(ability, ruleCalculateAbilityRange);
		}
		return ruleCalculateAbilityRange;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = (OverrideRange ?? DefaultRange) + Bonus + FiringArcBonus;
	}
}
