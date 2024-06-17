using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0d72f7816726475599b6790483a3f7c5")]
public class WarhammerReflectDamage : WarhammerDamageTrigger, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	[HideIf("UseValueInstead")]
	private ContextValue m_Percentage;

	[SerializeField]
	[ShowIf("UseValueInstead")]
	private ContextValue m_Value;

	public bool UseValueInstead;

	public bool ChangeReflectedDamageType;

	[SerializeField]
	[ShowIf("ChangeReflectedDamageType")]
	private DamageType m_Type;

	public void OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
		int finalValue = rule.ResultValue.FinalValue;
		int value = ((!UseValueInstead) ? Mathf.RoundToInt(0.01f * (float)m_Percentage.Calculate(base.Context) * (float)finalValue) : ((finalValue >= m_Value.Calculate(base.Context)) ? m_Value.Calculate(base.Context) : finalValue));
		DamageType type = (ChangeReflectedDamageType ? m_Type : rule.Damage.Type);
		Rulebook.Trigger(new RuleDealDamage(rule.ConcreteTarget, rule.ConcreteInitiator, new DamageData(type, value)));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
