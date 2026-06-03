using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Covers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollCoverHit : RulebookOptionalTargetEvent
{
	[NotNull]
	public readonly RuleCalculateCoverHitChance HitChanceRule;

	public readonly RulePerformAttackRoll AttackRoll;

	private readonly bool m_IsAutoMissCover;

	public RuleRollD100 ResultD100 { get; private set; }

	public bool ResultIsHit { get; private set; }

	public RuleRollCoverHit([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [CanBeNull] AbilityData ability, LosCalculations.CoverType los, [CanBeNull] MechanicEntity cover, [CanBeNull] RulePerformAttackRoll attackRoll)
		: this(new RuleCalculateCoverHitChance(initiator, target, ability, los, cover), attackRoll)
	{
		base.HasNoTarget = false;
	}

	public RuleRollCoverHit([NotNull] RuleCalculateCoverHitChance hitChance, [CanBeNull] RulePerformAttackRoll attackRoll, bool isAutoMissCover = false)
		: base(hitChance.ConcreteInitiator, hitChance.MaybeTarget)
	{
		base.HasNoTarget = hitChance.MaybeTarget == null;
		m_IsAutoMissCover = isAutoMissCover;
		HitChanceRule = hitChance;
		AttackRoll = attackRoll;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(HitChanceRule);
		ResultD100 = Dice.D100;
		ResultIsHit = !m_IsAutoMissCover && !HitChanceRule.IsAutoMiss && (HitChanceRule.IsAutoHit || (int)ResultD100 <= HitChanceRule.ResultChance);
	}
}
