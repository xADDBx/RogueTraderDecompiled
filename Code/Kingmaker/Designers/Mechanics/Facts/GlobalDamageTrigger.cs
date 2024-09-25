using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ee0460f02f7745b0a9894e1ebe22370d")]
public class GlobalDamageTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnAttacker;

	public ActionList ActionsOnTarget;

	public ActionList ActionsOnCaster;

	public ConditionsChecker ConditionsOnTarget;

	public ConditionsChecker ConditionsOnAttacker;

	public ConditionsChecker ConditionsOnCaster;

	public bool TriggersForDamageOverTime;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth == null || evt.HPBeforeDamage <= 0 || !m_Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility))
		{
			return;
		}
		BlueprintScriptableObject blueprintScriptableObject = evt.Reason.Context?.AssociatedBlueprint;
		if ((blueprintScriptableObject is BlueprintBuff || blueprintScriptableObject is BlueprintAbilityAreaEffect) && !TriggersForDamageOverTime)
		{
			return;
		}
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity mechanicEntity = (MechanicEntity)evt.Initiator;
		MechanicEntity mechanicEntity2 = (MechanicEntity)evt.Target;
		if (maybeCaster != null && mechanicEntity != null && mechanicEntity2 != null)
		{
			bool flag;
			using (base.Context.GetDataScope(maybeCaster.ToITargetWrapper()))
			{
				flag = ConditionsOnCaster.Check();
			}
			bool flag2;
			using (base.Context.GetDataScope(mechanicEntity.ToITargetWrapper()))
			{
				flag2 = ConditionsOnAttacker.Check();
			}
			bool flag3;
			using (base.Context.GetDataScope(mechanicEntity2.ToITargetWrapper()))
			{
				flag3 = ConditionsOnTarget.Check();
			}
			if (flag2 && flag && flag3)
			{
				base.Fact.RunActionInContext(ActionsOnCaster, maybeCaster.ToITargetWrapper());
				base.Fact.RunActionInContext(ActionsOnTarget, mechanicEntity2.ToITargetWrapper());
				base.Fact.RunActionInContext(ActionsOnAttacker, mechanicEntity.ToITargetWrapper());
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
