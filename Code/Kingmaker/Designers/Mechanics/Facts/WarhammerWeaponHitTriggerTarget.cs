using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("66be5599926e57d45a7a016495d269c4")]
public class WarhammerWeaponHitTriggerTarget : WarhammerWeaponHitTriggerBase, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IFakeCriticalHandler, ISubscriber<IBaseUnitEntity>, IHashable
{
	public bool OnlyCritical;

	public bool OnlyFromSpotWeaknessSide;

	[SerializeField]
	[ShowIf("OnlyFromSpotWeaknessSide")]
	private BlueprintBuffReference m_SpotWeaknessBuff;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool OnlyFromContextCaster;

	public BlueprintBuff SpotWeaknessBuff => m_SpotWeaknessBuff.Get();

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		CheckConditionsAndRunActions(evt.ConcreteInitiator, evt.ConcreteTarget, evt.Ability.Weapon, evt.RollPerformAttackRule.ResultIsRighteousFury || evt.RollPerformAttackRule.HitChanceRule.AutoCrits.Value || evt.RollPerformAttackRule.ShouldHaveBeenRighteousFury, evt.ResultIsHit);
	}

	private void CheckConditionsAndRunActions(MechanicEntity initiator, MechanicEntity target, ItemEntityWeapon weapon, bool isCritical, bool isHit)
	{
		if (OnlyFromSpotWeaknessSide)
		{
			bool flag = false;
			foreach (Buff item in target.Buffs.Enumerable.Where((Buff p) => p.Blueprint == SpotWeaknessBuff))
			{
				foreach (WarhammerBonusDamageFromSide item2 in item.SelectComponents<WarhammerBonusDamageFromSide>())
				{
					flag |= item2.CheckSide(initiator, target);
				}
			}
			if (!flag)
			{
				return;
			}
		}
		if ((!OnlyFromContextCaster || initiator == base.Context.MaybeCaster) && (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon))) && (!OnlyCritical || isCritical))
		{
			TryRunActions(initiator, target, isHit);
		}
	}

	public void HandleFakeCritical(RuleDealDamage evt)
	{
		BaseUnitEntity targetUnit = evt.TargetUnit;
		if (targetUnit == null || !OnlyCritical)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility))
			{
				return;
			}
		}
		CheckConditionsAndRunActions(evt.ConcreteInitiator, targetUnit, evt.SourceAbility?.Weapon, isCritical: true, isHit: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
