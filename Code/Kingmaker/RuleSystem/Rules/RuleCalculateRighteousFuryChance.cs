using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateRighteousFuryChance : RulebookOptionalTargetEvent, IDamageHolderRule
{
	public readonly ValueModifiersManager ChanceModifiers = new ValueModifiersManager();

	public readonly PercentsMultipliersManager ChancePercentMultipliers = new PercentsMultipliersManager();

	public readonly PercentsModifiersManager ChancePercentModifiers = new PercentsModifiersManager();

	public AbilityData Ability { get; }

	public int RawResult { get; private set; }

	public int ResultChance { get; private set; }

	public int BonusCriticalChance { get; private set; }

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in ChanceModifiers.List)
			{
				yield return item;
			}
			foreach (Modifier item2 in ChancePercentModifiers.List)
			{
				yield return item2;
			}
			foreach (Modifier item3 in ChancePercentMultipliers.List)
			{
				yield return item3;
			}
		}
	}

	public ItemEntityWeapon Weapon => Ability.Weapon;

	public DamageData Damage => Ability.GetWeaponStats().ResultDamage;

	public RuleCalculateRighteousFuryChance([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [NotNull] AbilityData ability)
		: base(initiator, target)
	{
		Ability = ability;
		if (!Ability.IsMelee)
		{
			ChanceModifiers.Add(BlueprintRoot.Instance.WarhammerRoot.CombatRoot.BaseRighteousFury, this, ModifierDescriptor.None);
		}
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		BonusCriticalChance = (int)((float)ChanceModifiers.Value * ChancePercentModifiers.Value);
		RawResult = Mathf.RoundToInt((float)BonusCriticalChance * ChancePercentMultipliers.Value);
		ResultChance = Math.Clamp(RawResult, 0, 100);
	}
}
