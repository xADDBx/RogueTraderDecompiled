using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("e5e0702f76904c81af652956dfd515ff")]
public class OverrideAbilityIgnoreLOS : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityNeedLOS>, IRulebookHandler<RuleCalculateAbilityNeedLOS>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_AbilityGroupList;

	public bool NotSelectedGroup;

	private ReferenceArrayProxy<BlueprintAbilityGroup> AbilityGroupList
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] abilityGroupList = m_AbilityGroupList;
			return abilityGroupList;
		}
	}

	public bool ShouldIgnoreLOS(EntityFactComponent runtime, AbilityData abilityData)
	{
		if (!Restrictions.IsPassed((MechanicEntityFact)runtime.Fact, runtime.Fact.MaybeContext, null, abilityData))
		{
			return false;
		}
		BlueprintAbility blueprint = abilityData.Blueprint;
		if (m_AbilityGroupList.Length != 0 && AbilityGroupIsSelected(blueprint.AbilityGroups) == NotSelectedGroup)
		{
			return false;
		}
		return true;
	}

	private bool AbilityGroupIsSelected(ReferenceArrayProxy<BlueprintAbilityGroup> abilityGroups)
	{
		foreach (BlueprintAbilityGroup item in abilityGroups)
		{
			if (AbilityGroupList.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityNeedLOS evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (m_AbilityGroupList.Length == 0 || AbilityGroupIsSelected(evt.Ability.Blueprint.AbilityGroups) != NotSelectedGroup))
		{
			evt.IgnoreLOS.Add(base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityNeedLOS evt)
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
