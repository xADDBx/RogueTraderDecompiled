using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
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
[TypeId("655d56d2a908e0846928313013cf0fc9")]
public class WarhammerRighteousFuryBonusTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue Value;

	public ContextValue Multiplier;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool OnlyFromSpotWeaknessSide;

	[SerializeField]
	[ShowIf("OnlyFromSpotWeaknessSide")]
	private BlueprintBuffReference m_SpotWeaknessBuff;

	public BlueprintBuff SpotWeaknessBuff => m_SpotWeaknessBuff.Get();

	public void OnEventAboutToTrigger(RuleCalculateRighteousFuryChance evt)
	{
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
		float num = ((Multiplier.Calculate(base.Context) == 0) ? 1 : Multiplier.Calculate(base.Context));
		if (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon)))
		{
			evt.ChanceModifiers.Add((int)((float)Value.Calculate(base.Context) * num), base.Fact);
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
