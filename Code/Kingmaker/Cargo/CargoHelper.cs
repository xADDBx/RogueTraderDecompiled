using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Items;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Cargo;

public static class CargoHelper
{
	public const int MaxVolumePercent = 100;

	public static readonly IEnumerable<ItemsItemOrigin> Origins = Enum.GetValues(typeof(ItemsItemOrigin)).Cast<ItemsItemOrigin>();

	public static int MaxFilledVolumePercentToAddItem => Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.MaxFilledVolumePercentToAddItem;

	public static bool CanTransferFromCargo(ItemEntity item)
	{
		if (item != null && !IsTrashItem(item))
		{
			return !Game.Instance.Player.CargoState.LockTransferFromCargo;
		}
		return false;
	}

	public static bool CanTransferToCargo(ItemEntity item)
	{
		if (item != null && !IsQuestItem(item.Blueprint))
		{
			return HasOrigin(item.Blueprint);
		}
		return false;
	}

	public static bool IsItemInCargo(ItemEntity item)
	{
		if (item != null)
		{
			ItemsCollection collection = item.Collection;
			if (collection != null)
			{
				return collection.Owner is CargoEntity;
			}
		}
		return false;
	}

	public static bool IsTrashItem(ItemEntity item)
	{
		if (item != null)
		{
			return IsTrashItem(item.Blueprint);
		}
		return false;
	}

	public static bool IsTrashItem(BlueprintItem item)
	{
		if (item != null && item.GetType() == typeof(BlueprintItem))
		{
			return !item.IsNotable;
		}
		return false;
	}

	public static bool IsTrashItemWithoutOrigin(BlueprintItem item)
	{
		if (item != null && IsTrashItem(item))
		{
			return !HasOrigin(item);
		}
		return false;
	}

	public static bool HasOrigin(BlueprintItem item)
	{
		if (item != null)
		{
			return item.Origin != ItemsItemOrigin.None;
		}
		return false;
	}

	public static bool IsQuestItem(BlueprintItem item)
	{
		if (item != null && item.GetType() == typeof(BlueprintItem))
		{
			return item.IsNotable;
		}
		return false;
	}

	[Cheat(Name = "add_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddCargo([NotNull] string blueprintName)
	{
		BlueprintCargo blueprint = Utilities.GetBlueprint<BlueprintCargo>(blueprintName);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find cargo blueprint by name: {0}", blueprintName);
		}
		else
		{
			Game.Instance.Player.CargoState.Create(blueprint);
		}
	}

	[Cheat(Name = "remove_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RemoveCargo([NotNull] string blueprintName)
	{
		BlueprintCargo blueprint = Utilities.GetBlueprint<BlueprintCargo>(blueprintName);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find cargo blueprint by name: {0}", blueprintName);
		}
		else
		{
			Game.Instance.Player.CargoState.Remove(blueprint);
		}
	}

	[Cheat(Name = "move_open_loot_to_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void LootToCargo([NotNull] string blueprintName)
	{
		if (!Game.Instance.RootUiContext.SurfaceVM.StaticPartVM.LootContextVM.IsShown)
		{
			PFLog.SmartConsole.Log("Loot view not shown");
			return;
		}
		foreach (LootObjectVM item in Game.Instance.RootUiContext.SurfaceVM.StaticPartVM.LootContextVM.LootVM.Value.LootCollector.ContextLoot)
		{
			Game.Instance.Player.CargoState.AddToCargo(item.ItemEntities);
		}
	}

	[Cheat(Name = "sell_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SellCargo([NotNull] string blueprintName, FactionType faction)
	{
		BlueprintCargo blueprint = Utilities.GetBlueprint<BlueprintCargo>(blueprintName);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Cannot find cargo blueprint by name: {0}", blueprintName);
			return;
		}
		CargoEntity cargoEntity = Game.Instance.Player.CargoState.Get(blueprint, (CargoEntity entity) => entity.IsFull).FirstOrDefault();
		if (cargoEntity == null)
		{
			PFLog.SmartConsole.Log("Cannot find full cargo by name: {0}", blueprintName);
		}
		else
		{
			Game.Instance.Player.CargoState.SellCargoes(new List<CargoEntity> { cargoEntity }, faction);
		}
	}

	[Cheat(Name = "add_random_cargo_to_sell", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddRandomCargoToSell()
	{
		VendorLogic vendorLogic = VendorHelper.Vendor;
		CargoEntity cargoEntity = Game.Instance.Player.CargoState.CargoEntities.Where((CargoEntity x) => vendorLogic.CanAddToSell(x)).Random(PFStatefulRandom.Cargo);
		if (cargoEntity != null)
		{
			VendorHelper.Vendor.AddToSell(cargoEntity);
		}
	}

	[Cheat(Name = "remove_random_cargo_from_sell", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RemoveRandomCargoFromSell()
	{
		VendorLogic vendorLogic = VendorHelper.Vendor;
		CargoEntity cargoEntity = Game.Instance.Player.CargoState.CargoEntities.Where((CargoEntity x) => vendorLogic.CanRemoveFromSell(x)).Random(PFStatefulRandom.Cargo);
		if (cargoEntity != null)
		{
			VendorHelper.Vendor.RemoveFromSell(cargoEntity);
		}
	}

	[Cheat(Name = "deal_sell_cargoes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void DealSellCargoes()
	{
		VendorHelper.Vendor.DealSellCargoes(VendorHelper.Vendor.VendorEntity);
	}

	[Cheat(Name = "cancel_sell_cargoes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CancelSellCargoesDeal()
	{
		VendorHelper.Vendor.CancelSellCargoesDeal();
	}

	[Cheat(Name = "fill_random_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void FillRandomCargo(int targetVolume = 100, string blueprintName = null)
	{
		CargoEntity cargoEntity = GetNotFilledCargoEntities(targetVolume, blueprintName).Random(PFStatefulRandom.Cargo);
		cargoEntity?.AddUnusableVolumePercent(targetVolume - cargoEntity.FilledVolumePercent);
	}

	[Cheat(Name = "fill_all_cargoes", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void FillAllCargoes(int targetVolume = 100, string blueprintName = null)
	{
		foreach (CargoEntity notFilledCargoEntity in GetNotFilledCargoEntities(targetVolume, blueprintName))
		{
			notFilledCargoEntity.AddUnusableVolumePercent(targetVolume - notFilledCargoEntity.FilledVolumePercent);
		}
	}

	[Cheat(Name = "add_random_cargo", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddRandomCargo()
	{
		AddCargo((from x in Utilities.GetBlueprintGuids<BlueprintCargo>()
			where x != null
			select x).Random(PFStatefulRandom.Cargo));
	}

	public static IEnumerable<CargoEntity> GetNotFilledCargoEntities(int targetVolume = 100, string blueprintName = null)
	{
		if (blueprintName == null)
		{
			return Game.Instance.Player.CargoState.CargoEntities.Where((CargoEntity x) => x.FilledVolumePercent < targetVolume);
		}
		BlueprintCargo blueprintCargo = Utilities.GetBlueprint<BlueprintCargo>(blueprintName);
		if (blueprintCargo != null)
		{
			return Game.Instance.Player.CargoState.CargoEntities.Where((CargoEntity x) => x.FilledVolumePercent < targetVolume && x.Blueprint == blueprintCargo);
		}
		PFLog.SmartConsole.Log("Cannot find cargo blueprint by name: {0}", blueprintName);
		return Array.Empty<CargoEntity>();
	}
}
