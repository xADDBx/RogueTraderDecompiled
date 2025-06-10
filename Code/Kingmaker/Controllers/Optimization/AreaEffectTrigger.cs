using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Controllers.Optimization;

public class AreaEffectTrigger : MonoBehaviour
{
	public readonly HashSet<UnitReference> Inside = new HashSet<UnitReference>();

	public readonly HashSet<UnitReference> Entered = new HashSet<UnitReference>();

	public readonly HashSet<UnitReference> Exited = new HashSet<UnitReference>();

	public AreaEffectEntity Unit { get; set; }

	public void ClearDelta()
	{
		Entered.Clear();
		Exited.Clear();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity { IsExtra: false } baseUnitEntity && !WasAlreadyInAreaEffectCluster(baseUnitEntity))
		{
			if (!Inside.Add(baseUnitEntity.FromBaseUnitEntity()))
			{
				EntityBoundsController.Logger.Error("Cant add {0} to vision of {1} - already added", baseUnitEntity, Unit);
			}
			else
			{
				Entered.Add(baseUnitEntity.FromBaseUnitEntity());
				Unit?.HandleEntityPositionChanged();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (EntityDataLink.GetEntity(other) is BaseUnitEntity { IsExtra: false } baseUnitEntity && !WillRemainInAreaEffectCluster(baseUnitEntity))
		{
			if (!Inside.Remove(baseUnitEntity.FromBaseUnitEntity()))
			{
				EntityBoundsController.Logger.Error("Cant remove {0} to vision of {1} - never added", baseUnitEntity, Unit);
			}
			else
			{
				Exited.Add(baseUnitEntity.FromBaseUnitEntity());
				Unit?.HandleEntityPositionChanged();
			}
		}
	}

	private bool WasAlreadyInAreaEffectCluster(BaseUnitEntity unitInRange)
	{
		AreaEffectClusterComponent component = Unit.Blueprint.GetComponent<AreaEffectClusterComponent>();
		if (component == null)
		{
			return false;
		}
		if (IsInAnotherAreaEffect(unitInRange, component.ClusterLogicBlueprint, Unit))
		{
			return true;
		}
		return false;
	}

	private bool WillRemainInAreaEffectCluster(BaseUnitEntity unitInRange)
	{
		AreaEffectClusterComponent component = Unit.Blueprint.GetComponent<AreaEffectClusterComponent>();
		if (component == null)
		{
			return false;
		}
		if (IsInAnotherAreaEffect(unitInRange, component.ClusterLogicBlueprint, Unit))
		{
			unitInRange.GetPartUnitInAreaEffectClusterOptional()?.RemoveExitingAreaEffectFromList(component.ClusterLogicBlueprint, Unit);
			return true;
		}
		unitInRange.GetPartUnitInAreaEffectClusterOptional()?.RemoveClusterKey(component.ClusterLogicBlueprint);
		return false;
	}

	private static bool IsInAnotherAreaEffect(BaseUnitEntity unitInRange, BlueprintAbilityAreaEffectClusterLogic clusterKey, AreaEffectEntity areaEffectEntity)
	{
		if (!unitInRange.HasCurrentClusterKey(clusterKey))
		{
			return false;
		}
		if (unitInRange.IsCurrentlyInAnotherClusterArea(clusterKey, areaEffectEntity))
		{
			return true;
		}
		return false;
	}
}
