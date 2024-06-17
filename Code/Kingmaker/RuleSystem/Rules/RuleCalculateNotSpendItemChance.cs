using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateNotSpendItemChance : RulebookEvent
{
	public readonly PercentsModifiersManager NotSpendItemChanceModifier = new PercentsModifiersManager();

	public AbilityData Ability;

	public bool Success;

	public RuleRollChance RollChanceRule { get; private set; }

	public RuleCalculateNotSpendItemChance([NotNull] IMechanicEntity initiator, [NotNull] AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		float num = Mathf.Clamp(NotSpendItemChanceModifier.Bonus * 100f, 0f, 100f);
		RollChanceRule = Rulebook.Trigger(new RuleRollChance(base.Initiator, (int)num));
		Success = RollChanceRule.Success;
	}
}
