using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bd637fd86978432bae77b7a9cd4bb245")]
public class ChangeRuleOnAttack : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool ChangeDamageValue;

	[ShowIf("ChangeDamageValue")]
	public ContextValue ContextAdditionDamageValue;

	public bool ChangeCritChance;

	[ShowIf("ChangeCritChance")]
	public int AdditionCritChance;

	public bool ChangeCritDamage;

	[ShowIf("ChangeCritDamage")]
	public int AdditionCritDamage;

	public ActionList Actions;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (evt == null)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		ActionList actions = Actions;
		if (actions != null && actions.HasActions)
		{
			base.Fact.RunActionInContext(Actions);
		}
		if (ChangeDamageValue)
		{
			evt.ValueModifiers?.Add(ModifierType.ValAdd, ContextAdditionDamageValue.Calculate(base.Context), base.Fact);
		}
		if (ChangeCritChance)
		{
			evt.RollPerformAttackRule?.HitChanceRule?.RighteousFuryChanceRule?.ChanceModifiers?.Add(AdditionCritChance, base.Fact);
		}
		if (ChangeCritDamage)
		{
			evt.CriticalDamageModifiers?.Add(ModifierType.ValAdd, AdditionCritDamage, base.Fact);
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
