using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/HealthLevelTrigger")]
[AllowMultipleComponents]
[TypeId("d74c5a91c6b41cc4292e249970fa7c49")]
public class HealthLevelTrigger : MechanicEntityFactComponentDelegate, IDamageHandler, ISubscriber, IUnitContextActionKillHandler, ISubscriber<IBaseUnitEntity>, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[HideIf("UseValueInstead")]
	public int Percentage;

	[ShowIf("UseValueInstead")]
	public int Value;

	public bool UseValueInstead;

	public ActionList Actions;

	public void HandleDamageDealt(RuleDealDamage dealDamage)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!m_Restrictions.IsPassed(base.Fact, dealDamage, dealDamage.SourceAbility))
			{
				return;
			}
		}
		if (ShouldTriggerHealthLevel(dealDamage))
		{
			base.Fact.RunActionInContext(Actions, (TargetWrapper)(MechanicEntity)dealDamage.Target);
			base.ExecutesCount++;
		}
	}

	private bool ShouldTriggerHealthLevel(RuleDealDamage dealDamage)
	{
		PartHealth healthOptional = dealDamage.Target.GetHealthOptional();
		if (base.Owner != dealDamage.Target || healthOptional == null)
		{
			return false;
		}
		int num = (UseValueInstead ? Value : ((Percentage != 0) ? Math.Max(1, healthOptional.PercentToHitPoints(Percentage)) : 0));
		int hitPointsLeft = healthOptional.HitPointsLeft;
		bool flag = hitPointsLeft + dealDamage.Result > num;
		bool flag2 = hitPointsLeft <= num;
		return flag && flag2;
	}

	public void HandleOnContextActionKill(MechanicEntity caster, MechanicEntity target, BlueprintMechanicEntityFact blueprint, RulePerformSavingThrow rule)
	{
		PartHealth healthOptional = target.GetHealthOptional();
		if (base.Owner != target || healthOptional == null)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			RuleDealDamage rule2 = new RuleDealDamage(caster, target, new DamageData(DamageType.Direct, healthOptional.MaxHitPoints));
			if (m_Restrictions.IsPassed(base.Fact, rule2))
			{
				int num = (UseValueInstead ? Value : healthOptional.PercentToHitPoints(Percentage));
				_ = healthOptional.HitPointsLeft;
				if (num > healthOptional.HitPointsLeft)
				{
					base.Fact.RunActionInContext(Actions, target);
					base.ExecutesCount++;
				}
			}
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
