using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartCultAmbushExtension
{
	public static bool TryGetUnitPartCultAmbush(this BaseUnitEntity entity, out UnitPartCultAmbush ambush)
	{
		ambush = entity.GetOptional<UnitPartCultAmbush>();
		return ambush != null;
	}

	public static UnitPartCultAmbush.VisibilityStatuses CultAmbushVisibility(this Ability ability, bool isFirstShow = false)
	{
		return (ability.ConcreteOwner?.GetOptional<UnitPartCultAmbush>())?.Visibility(ability, isFirstShow) ?? UnitPartCultAmbush.VisibilityStatuses.Visible;
	}

	public static UnitPartCultAmbush.VisibilityStatuses CultAmbushVisibility(this BlueprintAbility blueprintAbility, BaseUnitEntity unit, bool isFirstShow = false)
	{
		if (unit == null || !unit.TryGetUnitPartCultAmbush(out var ambush))
		{
			return UnitPartCultAmbush.VisibilityStatuses.Visible;
		}
		return ambush.Visibility(unit, blueprintAbility, isFirstShow);
	}

	public static UnitPartCultAmbush.VisibilityStatuses CultAmbushVisibility(this Feature feature, bool isFirstShow = false)
	{
		return (feature.ConcreteOwner?.GetOptional<UnitPartCultAmbush>())?.Visibility(feature, isFirstShow) ?? UnitPartCultAmbush.VisibilityStatuses.Visible;
	}

	public static UnitPartCultAmbush.VisibilityStatuses CultAmbushVisibility(this BlueprintFeature blueprintFeature, BaseUnitEntity unit, bool isFirstShow = false)
	{
		if (unit == null || !unit.TryGetUnitPartCultAmbush(out var ambush))
		{
			return UnitPartCultAmbush.VisibilityStatuses.Visible;
		}
		return ambush.Visibility(unit, blueprintFeature, isFirstShow);
	}
}
