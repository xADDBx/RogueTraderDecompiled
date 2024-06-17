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

	private bool m_IsAutoHit;

	public RuleRollD100 ResultD100 { get; private set; }

	public bool ResultIsHit { get; private set; }

	public LosCalculations.CoverType Los => HitChanceRule.Los;

	[CanBeNull]
	public MechanicEntity Cover => HitChanceRule.Cover;

	public int BaseChance => HitChanceRule.BaseChance;

	public int ResultChance => HitChanceRule.ResultChance;

	public RuleRollCoverHit([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [CanBeNull] AbilityData ability, LosCalculations.CoverType los, [CanBeNull] MechanicEntity cover)
		: this(new RuleCalculateCoverHitChance(initiator, target, ability, los, cover))
	{
	}

	public RuleRollCoverHit([NotNull] RuleCalculateCoverHitChance hitChance, bool isAutoHit = false)
		: base(hitChance.ConcreteInitiator, hitChance.MaybeTarget)
	{
		m_IsAutoHit = isAutoHit;
		HitChanceRule = hitChance;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(HitChanceRule);
		ResultD100 = Dice.D100;
		ResultIsHit = !m_IsAutoHit && (int)ResultD100 <= ResultChance;
	}
}
