using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateGroupCooldown : RulebookEvent
{
	public BlueprintAbilityGroup AbilityGroup { get; private set; }

	public int Result { get; set; }

	public RuleCalculateCooldown BaseCooldownRule { get; private set; }

	public RuleCalculateGroupCooldown([NotNull] MechanicEntity initiator, BlueprintAbilityGroup group)
		: base(initiator)
	{
		AbilityGroup = group;
	}

	public RuleCalculateGroupCooldown([NotNull] MechanicEntity initiator, BlueprintAbilityGroup group, RuleCalculateCooldown baseCooldownRule)
		: base(initiator)
	{
		AbilityGroup = group;
		BaseCooldownRule = baseCooldownRule;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = AbilityGroup?.CooldownInRounds ?? 0;
	}
}
