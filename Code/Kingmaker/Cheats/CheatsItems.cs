using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsItems
{
	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("AoELoot", delegate
			{
				CheatsHelper.Run("aoe_loot");
			});
			SmartConsole.RegisterCommand("use_item", UseItem);
			SmartConsole.RegisterCommand("list_items", ListItems);
			SmartConsole.RegisterCommand("drop_heavy_items", DropHeavyItems);
			SmartConsole.RegisterCommand("drop_groups_items", DropItems);
			SmartConsole.RegisterCommand("priority_class", "Sets equipment priority class on main character. No argument to reset", SetPriorityEquipClass);
		}
	}

	private static void DropHeavyItems(string parameters)
	{
		ItemsCollection inventory = Game.Instance.Player.Inventory;
		List<ItemEntity> list = inventory.Items.Where((ItemEntity x) => x.HoldingSlot == null).ToTempList();
		list.Sort((ItemEntity a, ItemEntity b) => b.TotalWeight.CompareTo(a.TotalWeight));
		foreach (ItemEntity item in list)
		{
			if (EncumbranceHelper.GetPartyEncumbrance() != Encumbrance.Overload)
			{
				break;
			}
			inventory.DropItem(item);
		}
	}

	private static void DropItems(string parameters)
	{
		ItemsCollection inventory = Game.Instance.Player.Inventory;
		foreach (ItemEntity item in inventory.Items.Where((ItemEntity x) => x.HoldingSlot == null).ToTempList())
		{
			inventory.DropItem(item);
		}
	}

	private static void SetPriorityEquipClass(string parameters)
	{
		int? paramInt = Utilities.GetParamInt(parameters, 1, null);
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		for (int i = 0; i < playerCharacter.Progression.Classes.Count; i++)
		{
			playerCharacter.Progression.Classes[i].PriorityEquipment = paramInt == i;
		}
		playerCharacter.View.UpdateClassEquipment();
	}

	private static void ListItems(string parameters)
	{
		string value = Utilities.GetParamString(parameters, 1, null) ?? "";
		foreach (BlueprintItem scriptableObject in Utilities.GetScriptableObjects<BlueprintItem>())
		{
			string blueprintPath = Utilities.GetBlueprintPath(scriptableObject);
			if (blueprintPath.Contains(value))
			{
				PFLog.SmartConsole.Log(blueprintPath);
			}
		}
	}

	private static void UseItem(string parameters)
	{
		BaseUnitEntity playerCharacter = GameHelper.GetPlayerCharacter();
		int? paramInt = Utilities.GetParamInt(parameters, 1, "Can't parse slot to use");
		if (!paramInt.HasValue)
		{
			return;
		}
		UsableSlot usableSlot = playerCharacter.Body.QuickSlots.ElementAtOrDefault(paramInt.Value);
		if (usableSlot == null || !usableSlot.HasItem)
		{
			PFLog.SmartConsole.Log("usableSlots == null || !usableSlots.HasItem");
			return;
		}
		AbilityData data = usableSlot.Item.Abilities.FirstOrDefault().Data;
		if (data.TargetAnchor == AbilityTargetAnchor.Owner)
		{
			playerCharacter.Commands.Run(new UnitUseAbilityParams(data, playerCharacter));
		}
		else
		{
			Game.Instance.SelectedAbilityHandler.SetAbility(data);
		}
	}

	[Cheat(Name = "aoe_loot", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AoeLoot()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			Vector3 pos = partyAndPet.Position;
			foreach (BaseUnitEntity item in from u in Game.Instance.State.AllBaseUnits
				where u.IsDeadAndHasLoot
				where Vector3.Distance(u.Position, pos) < 30f
				select u)
			{
				foreach (ItemEntity item2 in item.Inventory.Items)
				{
					if (item2.IsLootable)
					{
						GameHelper.GetPlayerCharacter().Inventory.Add(item2);
						PFLog.SmartConsole.Log(item2.Name);
					}
				}
				item.Inventory.RemoveAll();
			}
		}
	}
}
