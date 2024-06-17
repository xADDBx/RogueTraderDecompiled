using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67bded0d11ea5094b86798ea2fce7c63")]
public class WarhammerRighteousFuryBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValue Value;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool OnlyFromSpotWeaknessSide;

	[SerializeField]
	[ShowIf("OnlyFromSpotWeaknessSide")]
	private BlueprintBuffReference m_SpotWeaknessBuff;

	public bool DoubleCurrentChance;

	public BlueprintBuff SpotWeaknessBuff => m_SpotWeaknessBuff.Get();

	public void OnEventAboutToTrigger(RuleCalculateRighteousFuryChance evt)
	{
		if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			return;
		}
		ItemEntityWeapon weapon = evt.Ability.Weapon;
		if (OnlyFromSpotWeaknessSide && evt.MaybeTarget != null)
		{
			bool flag = false;
			foreach (Buff item in evt.MaybeTarget.Buffs.Enumerable.Where((Buff p) => p.Blueprint == SpotWeaknessBuff))
			{
				foreach (WarhammerBonusDamageFromSide item2 in item.SelectComponents<WarhammerBonusDamageFromSide>())
				{
					flag |= item2.CheckSide(evt.ConcreteInitiator, evt.MaybeTarget);
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if (DoubleCurrentChance)
		{
			evt.ChancePercentModifiers.Add(100, base.Fact);
		}
		if (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon)))
		{
			evt.ChanceModifiers.Add(Value.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateRighteousFuryChance evt)
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
