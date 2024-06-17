using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowMultipleComponents]
[TypeId("ce207479288128c47ad759fa5285b967")]
public class AbilityTargetHasCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public UnitCondition Condition;

	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (target.Entity == null)
		{
			return false;
		}
		bool flag = target.Entity.HasCondition(Condition);
		if (!(!Not && flag))
		{
			if (Not)
			{
				return !flag;
			}
			return false;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoCondition : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasCondition).ToString(delegate
		{
			GameLogContext.Text = UIUtilityTexts.GetConditionText(Condition);
		});
	}
}
