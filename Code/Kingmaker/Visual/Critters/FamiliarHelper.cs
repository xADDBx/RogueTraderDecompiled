using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceManagement;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public static class FamiliarHelper
{
	public static bool IsFamiliarFact(this BlueprintUnitFact fact)
	{
		return fact?.GetComponent<AddFamiliar>() != null;
	}

	public static bool IsFamiliarItem(this BlueprintItem item)
	{
		return (item.GetComponent<AddFactToEquipmentWielder>()?.Fact?.IsFamiliarFact()).GetValueOrDefault();
	}

	public static bool IsFamiliarItem(this ItemEntity item)
	{
		return item.Blueprint.IsFamiliarItem();
	}

	public static AbstractUnitEntity SpawnFamiliar(BaseUnitEntity leader, BlueprintUnit unit)
	{
		FamiliarSettings familiarSettings = unit.GetComponent<FamiliarSettingsOverride>()?.FamiliarSettings ?? Game.Instance.BlueprintRoot.FamiliarsRoot.DefaultFamiliarSettings;
		if (familiarSettings.SpawnOnLocationCondition.HasConditions)
		{
			using (leader.Context.GetDataScope())
			{
				if (!familiarSettings.SpawnOnLocationCondition.Check())
				{
					return null;
				}
			}
		}
		AbstractUnitEntity abstractUnitEntity;
		using (BundledResourceHandle<UnitEntityView> bundledResourceHandle = BundledResourceHandle<UnitEntityView>.Request(unit.Prefab.AssetId))
		{
			abstractUnitEntity = Game.Instance.EntitySpawner.SpawnLightweightUnit(unit, bundledResourceHandle.Object, leader.Position, Quaternion.identity, leader.HoldingState);
		}
		abstractUnitEntity?.GetOrCreate<UnitPartFamiliar>().Init(leader);
		return abstractUnitEntity;
	}

	public static void DeSpawnFamiliar(BaseUnitEntity leader, AbstractUnitEntity familiar, bool immediately)
	{
		FamiliarSettings familiarSettings = familiar.Blueprint.GetComponent<FamiliarSettingsOverride>()?.FamiliarSettings ?? Game.Instance.BlueprintRoot.FamiliarsRoot.DefaultFamiliarSettings;
		AbstractUnitEntityView view = familiar.View;
		if ((object)view != null)
		{
			view.enabled = false;
		}
		Entity.ViewHandlingOnDisposePolicyType viewHandlingOnDisposePolicy = ((!immediately) ? familiarSettings.ViewHandlingOnDisposePolicyType : Entity.ViewHandlingOnDisposePolicyType.Destroy);
		familiar.SetViewHandlingOnDisposePolicy(viewHandlingOnDisposePolicy);
		Game.Instance.EntityDestroyer.Destroy(familiar);
	}
}
