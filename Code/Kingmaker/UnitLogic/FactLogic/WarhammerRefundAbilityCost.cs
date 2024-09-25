using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("86cfc07f29bec904ab50406e1afe7640")]
public class WarhammerRefundAbilityCost : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public bool ForOneAbility;

	[ShowIf("ForOneAbility")]
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public bool ForMultipleAbilities;

	[ShowIf("ForMultipleAbilities")]
	[SerializeField]
	public List<BlueprintAbilityReference> Abilities;

	public bool ForAbilityGroup;

	[ShowIf("ForAbilityGroup")]
	public BlueprintAbilityGroupReference AbilityGroup;

	public bool refundAP;

	public BlueprintAbility Ability => m_Ability?.Get();

	private void RunAction(AbilityData ability, TargetWrapper _)
	{
		if ((!ForOneAbility || ability.Blueprint == Ability) && (!ForMultipleAbilities || Abilities.HasItem((BlueprintAbilityReference r) => r.Is(ability.Blueprint))) && (!ForAbilityGroup || ability.Blueprint.AbilityGroups.Contains(AbilityGroup)) && refundAP && base.Context.MaybeCaster != null)
		{
			base.Context.MaybeCaster.GetCombatStateOptional()?.GainYellowPoint(ability.CalculateActionPointCost(), base.Context);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		RunAction(evt.Spell, evt.SpellTarget);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
