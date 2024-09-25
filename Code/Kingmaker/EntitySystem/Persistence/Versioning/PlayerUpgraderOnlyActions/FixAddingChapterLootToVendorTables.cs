using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("4ffd0b6c9d8e49ab8236f8877261ca6e")]
public class FixAddingChapterLootToVendorTables : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSharedVendorTableReference m_Table;

	[ValidateNotNull]
	[SerializeField]
	private BlueprintUnitLootReference m_Loot;

	private BlueprintSharedVendorTable Table => m_Table?.Get();

	private BlueprintUnitLoot Loot => m_Loot?.Get();

	public override string GetCaption()
	{
		if (Table != null)
		{
			return $"Fix Loot not added properly to {Table}";
		}
		return "FixAddingChapterLootToVendorTables not configured properly";
	}

	protected override void RunActionOverride()
	{
		SharedVendorTables.TableData table = Game.Instance.Player.SharedVendorTables.GetTable(Table);
		if (table == null)
		{
			return;
		}
		bool flag = false;
		if (Loot == null)
		{
			return;
		}
		foreach (LootEntry item in Loot.GenerateItems())
		{
			if (!table.KnownLootItems.HasItem((VendorLootItem i) => i.Item == item.Item))
			{
				flag = true;
			}
			if (!table.Entries.HasItem((LootEntry i) => i.Item == item.Item && i.Count == item.Count))
			{
				table.Entries.Add(item);
				flag = true;
			}
		}
		if (!table.KnownLootItems.HasItem((VendorLootItem i) => i.Item != null) || flag)
		{
			table.KnownLootItems = SharedVendorTables.GetFixedItems(table.Loot);
		}
	}
}
