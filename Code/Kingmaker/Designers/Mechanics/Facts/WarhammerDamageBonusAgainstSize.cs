using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bc27cedfd3b9067429f761205435e48a")]
public class WarhammerDamageBonusAgainstSize : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue Value;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool UseForDifferentSizes;

	[HideIf("UseForDifferentSizes")]
	public Size EnemySize;

	[ShowIf("UseForDifferentSizes")]
	public List<Size> ValidEnemySizes = new List<Size>();

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			ItemEntityWeapon itemEntityWeapon = evt.Ability?.Weapon;
			if ((!SpecificRangeType || (itemEntityWeapon != null && WeaponRangeType.IsSuitableWeapon(itemEntityWeapon))) && evt.MaybeTarget is UnitEntity unitEntity && (UseForDifferentSizes ? ValidEnemySizes.Contains(unitEntity.State.Size) : (EnemySize == unitEntity.State.Size)))
			{
				evt.ValueModifiers.Add(ModifierType.ValAdd, Value.Calculate(base.Context), base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
