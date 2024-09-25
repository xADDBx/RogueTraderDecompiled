using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0080b272caeab6a4da8a53324fcd6116")]
public class BuffBeforeApplyTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintBuffReference[] m_BuffFromList = new BlueprintBuffReference[0];

	public bool OnlyFromCaster;

	public bool OnlyFromCasterWithFact;

	[ShowIf("OnlyFromCasterWithFact")]
	[SerializeField]
	private BlueprintUnitFactReference m_CasterFact;

	public ActionList Actions;

	public ReferenceArrayProxy<BlueprintBuff> BuffFromList
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffFromList = m_BuffFromList;
			return buffFromList;
		}
	}

	public BlueprintUnitFact CasterFact => m_CasterFact;

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (!evt.CanApply || !BuffFromList.Contains(evt.AppliedBuff.Blueprint) || (OnlyFromCaster && base.Fact.MaybeContext?.MaybeCaster != evt.Context?.MaybeCaster) || (OnlyFromCasterWithFact && !(evt.Context?.MaybeCaster?.Facts.Contains(CasterFact)).GetValueOrDefault()))
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt))
			{
				return;
			}
		}
		ActionList actions = Actions;
		if (actions != null && actions.HasActions)
		{
			base.Fact.RunActionInContext(Actions, evt.ConcreteInitiator);
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
