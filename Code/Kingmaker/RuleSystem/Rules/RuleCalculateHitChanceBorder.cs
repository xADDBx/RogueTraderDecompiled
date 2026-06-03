using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateHitChanceBorder : RulebookOptionalTargetEvent
{
	public const int DEFAULT_CHANCE_BORDER = 100;

	public readonly ValueModifiersManager ValueModifiers = new ValueModifiersManager();

	[CanBeNull]
	public AbilityData Ability { get; }

	public bool IsAutoHit { get; set; }

	public int BaseValue { get; private set; }

	public int ResultOverkillBorder { get; private set; }

	public int Result { get; private set; }

	private bool IsRanged
	{
		get
		{
			if (Ability != null && !Ability.IsMelee)
			{
				return !Ability.IsScatter;
			}
			return false;
		}
	}

	public RuleCalculateHitChanceBorder([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability)
		: base(initiator, target)
	{
		Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		BaseValue = (IsRanged ? BlueprintRoot.Instance.WarhammerRoot.CombatRoot.HitChanceOverkillBorder : 100);
		ResultOverkillBorder = Mathf.Clamp(BaseValue + ValueModifiers.Value, 0, 100);
		Result = (IsAutoHit ? 100 : ResultOverkillBorder);
	}
}
