using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("541d0ccf657d4727a05935ec281de46f")]
public class IgnoreOverpenetrationDamageDecreament : BlueprintComponent
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public bool Active(Ability ability, RulebookEvent rule, AbilityData abilityData)
	{
		return m_Restrictions.IsPassed(ability, rule, abilityData);
	}
}
