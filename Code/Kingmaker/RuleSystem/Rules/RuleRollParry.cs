using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollParry : RulebookTargetEvent<UnitEntity, MechanicEntity>
{
	public AbilityData Ability { get; }

	public RuleCalculateParryChance ChancesRule { get; }

	public RuleRollChance RollChanceRule { get; private set; }

	public bool DeflectResult { get; private set; }

	public bool Result { get; private set; }

	public MechanicEntity Attacker => base.Target;

	public UnitEntity Defender => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException Target
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public RuleRollParry([NotNull] UnitEntity defender, [NotNull] MechanicEntity attacker, [NotNull] AbilityData ability, int resultSuperiorityNumber, bool rangedParry = false, int hitChance = 0)
		: base(defender, attacker)
	{
		Ability = ability;
		ChancesRule = new RuleCalculateParryChance(defender, attacker, Ability, resultSuperiorityNumber, rangedParry, hitChance);
	}

	public RuleRollParry([NotNull] UnitEntity defender, [NotNull] IMechanicEntity attacker, [NotNull] AbilityData ability, int resultSuperiorityNumber)
		: this(defender, (MechanicEntity)attacker, ability, resultSuperiorityNumber)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(ChancesRule);
		RollChanceRule = Rulebook.Trigger(new RuleRollChance(Defender, ChancesRule.Result, RollType.Parry, RollChanceType.Untyped, null, Attacker));
		RuleRollChance ruleRollChance = Rulebook.Trigger(new RuleRollChance(Defender, ChancesRule.DeflectionResult, RollType.Parry, RollChanceType.Untyped, null, Attacker));
		DeflectResult = ruleRollChance.Success;
		Result = RollChanceRule.Success;
	}
}
