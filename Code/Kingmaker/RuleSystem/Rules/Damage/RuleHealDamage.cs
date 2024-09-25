using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleHealDamage : RulebookTargetEvent
{
	public readonly RuleCalculateHeal CalculateHealRule;

	[CanBeNull]
	public readonly PartHealth TargetHealth;

	public int Value => CalculateHealRule.Value;

	public AbilityData Ability { get; set; }

	public RuleHealDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, int bonus)
		: this(initiator, target, default(DiceFormula), bonus)
	{
	}

	public RuleHealDamage([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, DiceFormula dice, int bonus)
		: this((MechanicEntity)initiator, (MechanicEntity)target, dice, bonus)
	{
	}

	public RuleHealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, int bonus)
		: this(initiator, target, default(DiceFormula), bonus)
	{
	}

	public RuleHealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, DiceFormula dice, int bonus, AbilityData abilityData = null)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		CalculateHealRule = new RuleCalculateHeal(initiator, target, dice, bonus);
		Ability = abilityData;
	}

	public RuleHealDamage([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, int min, int max, int bonus, AbilityData abilityData = null)
		: base(initiator, target)
	{
		TargetHealth = target.GetHealthOptional();
		CalculateHealRule = new RuleCalculateHeal(initiator, target, min, max, bonus);
		Ability = abilityData;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (TargetHealth != null && !this.SkipBecauseOfShadow())
		{
			Rulebook.Trigger(CalculateHealRule);
			TargetHealth.HealDamage(Value);
			EventBus.RaiseEvent(delegate(IHealingHandler h)
			{
				h.HandleHealing(this);
			});
		}
	}
}
