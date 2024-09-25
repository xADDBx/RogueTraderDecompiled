using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("40457d87ab694f899cc7523067b09fda")]
public class AbilityNotSpendItemChanceModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateNotSpendItemChance>, IRulebookHandler<RuleCalculateNotSpendItemChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private List<BlueprintAbilityReference> m_Abilities;

	[SerializeField]
	private List<BlueprintAbilityGroupReference> m_AbilityGroups;

	[SerializeField]
	private ContextValue m_NotSpendChance;

	public List<BlueprintAbility> Abilities => m_Abilities?.Dereference()?.EmptyIfNull().ToList();

	public List<BlueprintAbilityGroup> AbilityGroups => m_AbilityGroups?.Dereference()?.EmptyIfNull().ToList();

	public void OnEventAboutToTrigger(RuleCalculateNotSpendItemChance evt)
	{
		if (!m_Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			return;
		}
		if (Abilities.Contains(evt.Ability.Blueprint))
		{
			evt.NotSpendItemChanceModifier.Add(m_NotSpendChance.Calculate(base.Context), base.Fact);
			return;
		}
		foreach (BlueprintAbilityGroup abilityGroup in AbilityGroups)
		{
			if (evt.Ability.AbilityGroups.Contains(abilityGroup))
			{
				evt.NotSpendItemChanceModifier.Add(m_NotSpendChance.Calculate(base.Context), base.Fact);
				break;
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateNotSpendItemChance evt)
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
