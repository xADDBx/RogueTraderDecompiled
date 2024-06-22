using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Enums;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("35bdf749faa52ec4cbe9a8e1e733ee7d")]
public class CheckAbilityPsychicPowerTypeGetter : PropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private PsychicPower m_PowerType;

	protected override int GetBaseValue()
	{
		AbilityData ability = base.PropertyContext.Ability;
		if (((object)ability == null || (ability.Blueprint.AbilityParamsSource & WarhammerAbilityParamsSource.PsychicPower) != 0) && base.PropertyContext.Ability?.Blueprint.PsychicPower == m_PowerType)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((m_PowerType == PsychicPower.Minor) ? "Minor" : "Major");
		return "Psychic Power is " + text;
	}
}
