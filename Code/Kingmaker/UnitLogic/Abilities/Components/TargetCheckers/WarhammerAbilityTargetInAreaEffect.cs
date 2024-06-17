using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target in area effect")]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("245c8bbfd839454cb3e6df4b1c12932e")]
public class WarhammerAbilityTargetInAreaEffect : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	public bool AnyStrategistZone;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = m_AreaEffect?.Get();
		if (blueprintAbilityAreaEffect == null && !AnyStrategistZone)
		{
			PFLog.Default.Error("Area effect not set");
			return true;
		}
		Vector3 point = target.Point;
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint == blueprintAbilityAreaEffect && areaEffect.Contains(point))
			{
				return true;
			}
			BlueprintAbilityAreaEffect blueprint = areaEffect.Blueprint;
			if (blueprint != null && blueprint.IsStrategistAbility && AnyStrategistZone && areaEffect.Contains(point))
			{
				return true;
			}
		}
		return (target.Entity?.GetOptional<UnitPartSpawnedAreaEffects>()?.Contains(blueprintAbilityAreaEffect)).GetValueOrDefault();
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetNotInAreaEffect.ToString(delegate
		{
			GameLogContext.Text = m_AreaEffect?.Get().Name;
		});
	}
}
