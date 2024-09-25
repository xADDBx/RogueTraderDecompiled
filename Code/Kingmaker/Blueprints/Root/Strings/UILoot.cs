using System;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Localization;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UILoot
{
	public LocalizedString Loot;

	public LocalizedString LootPlayerChest;

	public LocalizedString SendToPlayerChest;

	public LocalizedString LootOnArea;

	public LocalizedString CollectAll;

	public LocalizedString LeaveZone;

	public LocalizedString CollectAllAndLeaveZone;

	public LocalizedString LootManager;

	public LocalizedString SendToCargo;

	public LocalizedString SendToInventory;

	public LocalizedString SendAllToCargo;

	public LocalizedString SendAllToInventory;

	public LocalizedString ItemsLootObject;

	public LocalizedString ItemsLootObjectDescr;

	public LocalizedString TrashLootObject;

	public LocalizedString TrashLootObjectDescr;

	public LocalizedString CollectAllBeforeLeave;

	public LocalizedString SkillCheckTitle;

	public LocalizedString SkillCheckResult;

	public LocalizedString SkillCheckValueAgainst;

	public LocalizedString SkillCheckSkillValue;

	public LocalizedString DropZoneUnsupportedItem;

	public LocalizedString CargoCollectedFromLoot;

	public LocalizedString LootLockedState;

	public LocalizedString ExitDescription;

	public (string, string) GetLootObjectStrings(LootObjectType objectType)
	{
		return objectType switch
		{
			LootObjectType.Normal => (ItemsLootObject, ItemsLootObjectDescr), 
			LootObjectType.Trash => (TrashLootObject, TrashLootObjectDescr), 
			_ => (string.Empty, string.Empty), 
		};
	}

	public string GetLootName(LootContainerType type)
	{
		switch (type)
		{
		case LootContainerType.DefaultLoot:
		case LootContainerType.Environment:
		case LootContainerType.Chest:
		case LootContainerType.Unit:
			return Loot;
		case LootContainerType.PlayerChest:
			return LootPlayerChest;
		default:
			return Loot;
		}
	}

	public string GetLootNameByContext(LootContextVM.LootWindowMode mode)
	{
		switch (mode)
		{
		case LootContextVM.LootWindowMode.Short:
		case LootContextVM.LootWindowMode.ShortUnit:
		case LootContextVM.LootWindowMode.StandardChest:
			return Loot;
		case LootContextVM.LootWindowMode.PlayerChest:
			return LootPlayerChest;
		case LootContextVM.LootWindowMode.ZoneExit:
			return LootOnArea;
		default:
			return Loot;
		}
	}
}
