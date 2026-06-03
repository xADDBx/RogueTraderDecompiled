using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityNeedLOS : RulebookEvent
{
	public class Cache : RuleCache<AbilityData, RuleCalculateAbilityNeedLOS>
	{
	}

	public readonly FlagModifiersManager IgnoreLOS = new FlagModifiersManager();

	public bool Result { get; private set; }

	public AbilityData Ability { get; }

	public RuleCalculateAbilityNeedLOS([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
	}

	public static RuleCalculateAbilityNeedLOS TryGetCachedOrTrigger(AbilityData ability)
	{
		RuleCalculateAbilityNeedLOS ruleCalculateAbilityNeedLOS = RuleCache<AbilityData, RuleCalculateAbilityNeedLOS>.Get(ability);
		if (ruleCalculateAbilityNeedLOS == null)
		{
			ruleCalculateAbilityNeedLOS = Rulebook.Trigger(new RuleCalculateAbilityNeedLOS(ability.Caster, ability));
			RuleCache<AbilityData, RuleCalculateAbilityNeedLOS>.Set(ability, ruleCalculateAbilityNeedLOS);
		}
		return ruleCalculateAbilityNeedLOS;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = !ShouldIgnoreLOS();
	}

	private bool ShouldIgnoreLOS()
	{
		if (IgnoreLOS.Value)
		{
			return true;
		}
		foreach (IAbilityIgnoreLOS component in Ability.Blueprint.GetComponents<IAbilityIgnoreLOS>())
		{
			if (component.ShouldIgnoreLOS(Ability))
			{
				return true;
			}
		}
		return false;
	}
}
