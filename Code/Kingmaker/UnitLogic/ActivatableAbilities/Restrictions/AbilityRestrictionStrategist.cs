using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[TypeId("b8052dd143694fde860c5ca9b6c1b285")]
public class AbilityRestrictionStrategist : BlueprintComponent, IAbilityRestriction, IAbilityPatternRestriction
{
	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		PartyStrategistManager strategistManager = Game.Instance.Player.StrategistManager;
		if (strategistManager.AllowFirstRoundRule && Game.Instance.TurnController.CombatRound < 2 && !strategistManager.IsAlreadyCastedThisTurn(ability))
		{
			return true;
		}
		if (!strategistManager.IsCastRestricted)
		{
			return true;
		}
		return false;
	}

	public string GetAbilityRestrictionUIText()
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.IsOnCooldown;
	}

	public bool IsPatternRestrictionPassed(AbilityData ability, MechanicEntity caster, TargetWrapper target, out AbilityData.UnavailabilityReasonType unavailabilityReason)
	{
		unavailabilityReason = AbilityData.UnavailabilityReasonType.None;
		OrientedPatternData orientedPattern = ability.GetPatternSettings().GetOrientedPattern(ability, ability.Caster.GetNearestNodeXZ(), target.NearestNode);
		if (ability.Blueprint.GetComponent<AbilityRestrictionStrategist>() == null)
		{
			return true;
		}
		StrategistTacticsAreaEffectType? strategistTacticsAreaEffectType = ability.Blueprint.ElementsArray.OfType<ContextActionSpawnAreaEffect>().FirstOrDefault()?.AreaEffect.TacticsAreaEffectType;
		List<AreaEffectEntity> list = TempList.Get<AreaEffectEntity>();
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Overlaps(orientedPattern.Nodes) && areaEffect.Blueprint.IsStrategistAbility)
			{
				list.Add(areaEffect);
			}
		}
		int count = list.Count;
		bool flag = count <= 1 && (count == 0 || list[0].Blueprint.TacticsAreaEffectType == strategistTacticsAreaEffectType);
		bool flag2 = flag;
		unavailabilityReason = ((!flag2) ? AbilityData.UnavailabilityReasonType.StrategistZonesCantOverlap : AbilityData.UnavailabilityReasonType.None);
		return flag2;
	}

	public string GetAbilityPatternRestrictionUIText(AbilityData ability, MechanicEntity caster, TargetWrapper target)
	{
		return LocalizedTexts.Instance.Reasons.StrategistZonesCantOverlap;
	}
}
