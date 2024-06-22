using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Parts;

public static class PartUnitInAreaEffectClusterExtension
{
	[CanBeNull]
	public static PartUnitInAreaEffectCluster GetPartUnitInAreaEffectClusterOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitInAreaEffectCluster>();
	}

	public static bool HasCurrentClusterKey(this MechanicEntity entity, BlueprintAbilityAreaEffectClusterLogic blueprint)
	{
		return entity.GetPartUnitInAreaEffectClusterOptional()?.ClusterKeys.Contains(blueprint) ?? false;
	}

	public static bool IsCurrentlyInAnotherClusterArea(this MechanicEntity entity, BlueprintAbilityAreaEffectClusterLogic blueprint)
	{
		PartUnitInAreaEffectCluster partUnitInAreaEffectClusterOptional = entity.GetPartUnitInAreaEffectClusterOptional();
		if (partUnitInAreaEffectClusterOptional != null)
		{
			partUnitInAreaEffectClusterOptional.AreaEffectEntitiesInVisit.TryGetValue(blueprint, out var value);
			if (true)
			{
				if (value == null)
				{
					return false;
				}
				return value.Count > 1;
			}
		}
		return false;
	}
}
