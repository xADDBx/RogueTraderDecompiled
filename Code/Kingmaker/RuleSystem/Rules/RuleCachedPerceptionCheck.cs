using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCachedPerceptionCheck : RulePerformSkillCheck
{
	public bool IsTargetInvisible { get; set; }

	public new BaseUnitEntity Initiator => (BaseUnitEntity)base.Initiator;

	public RuleCachedPerceptionCheck([NotNull] BaseUnitEntity unit, int difficulty)
		: base(unit, StatType.SkillAwareness, difficulty)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (Initiator.CachedPerceptionRoll == 0)
		{
			Initiator.CachedPerceptionRoll = Dice.D100;
		}
		base.OnTrigger(context);
		if (base.ResultIsSuccess)
		{
			Initiator.CachedPerceptionRoll = 0;
		}
	}

	protected override RuleRollChance RollChanceRule()
	{
		return RuleRollChance.FromInt(Initiator, GetSuccessChance(), Initiator.CachedPerceptionRoll, base.StatType);
	}
}
