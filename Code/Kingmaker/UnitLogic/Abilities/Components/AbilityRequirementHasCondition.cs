using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("f99cbe15c48f4c8d8d09f619518999fb")]
public class AbilityRequirementHasCondition : BlueprintComponent, IAbilityRestriction
{
	public bool Not;

	public UnitCondition[] Conditions;

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		for (int i = 0; i < Conditions.Length; i++)
		{
			bool flag = ability.Caster.HasCondition(Conditions[i]);
			if ((!flag && !Not) || (flag && Not))
			{
				return false;
			}
		}
		return true;
	}

	public string GetAbilityRestrictionUIText()
	{
		LocalizedString obj = (Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.HasForbiddenCondition : BlueprintRoot.Instance.LocalizedTexts.Reasons.NoRequiredCondition);
		string conditions = string.Join(", ", Conditions.Select(UIUtilityTexts.GetConditionText));
		return obj.ToString(delegate
		{
			GameLogContext.Text = conditions;
		});
	}
}
