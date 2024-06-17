using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("65a7aea4342031044a5dfb98d710dc20")]
public class WarhammerDamageDealtToSharedValue : WarhammerDamageTrigger, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public AbilitySharedValue SharedValue;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool OnlyFromSpotWeaknessSide;

	[SerializeField]
	[ShowIf("OnlyFromSpotWeaknessSide")]
	private BlueprintBuffReference m_SpotWeaknessBuff;

	public BlueprintBuff SpotWeaknessBuff => m_SpotWeaknessBuff.Get();

	public void OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
		if (OnlyFromSpotWeaknessSide)
		{
			MechanicEntity obj = (MechanicEntity)rule.Target;
			bool flag = false;
			foreach (Buff item in obj.Buffs.Enumerable.Where((Buff p) => p.Blueprint == SpotWeaknessBuff))
			{
				foreach (WarhammerBonusDamageFromSide item2 in item.SelectComponents<WarhammerBonusDamageFromSide>())
				{
					flag |= item2.CheckSide(rule.ConcreteInitiator, rule.ConcreteTarget);
				}
			}
			if (!flag)
			{
				return;
			}
		}
		BlueprintItemWeapon contextDamageWeapon = rule.ContextDamageWeapon;
		if (!SpecificRangeType || (contextDamageWeapon != null && WeaponRangeType.IsSuitableWeapon(contextDamageWeapon)))
		{
			base.Context[SharedValue] += rule.Result;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
