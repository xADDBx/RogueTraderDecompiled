using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[TypeId("25c565ca84244ffd94ad766ff6fdf7da")]
public class AbilityRestrictionAnyAreaEffectsOverlapping : BlueprintComponent, IAbilityPatternRestriction
{
	public bool IsPatternRestrictionPassed(AbilityData ability, MechanicEntity caster, TargetWrapper target, out AbilityData.UnavailabilityReasonType unavailabilityReason)
	{
		unavailabilityReason = AbilityData.UnavailabilityReasonType.None;
		OrientedPatternData orientedPattern = ability.GetPatternSettings().GetOrientedPattern(ability, ability.Caster.GetNearestNodeXZ(), target.NearestNode);
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (areaEffect.Blueprint.SavePersistentArea && areaEffect.Overlaps(orientedPattern.Nodes))
			{
				unavailabilityReason = AbilityData.UnavailabilityReasonType.AreaEffectsCannotOverlap;
				return false;
			}
		}
		return true;
	}

	public string GetAbilityPatternRestrictionUIText(AbilityData ability, MechanicEntity caster, TargetWrapper target)
	{
		return LocalizedTexts.Instance.Reasons.AreaEffectsCannotOverlap;
	}
}
