using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has stat")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("d031c0cbc9462c947a066310de0283e6")]
public class AbilityTargetStatCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public StatType Stat;

	public int GreaterThan;

	public bool Inverted;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return false;
		}
		ModifiableValue statOptional = entity.GetStatOptional(Stat);
		if (statOptional == null)
		{
			return false;
		}
		if (statOptional.BaseValue > GreaterThan)
		{
			return !Inverted;
		}
		return Inverted;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return (Inverted ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetStatConditionLowerOrEqual : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetStatCondition).ToString(delegate
		{
			GameLogContext.Text = UIUtility.GetStatText(Stat);
			GameLogContext.Description = GreaterThan.ToString();
		});
	}
}
