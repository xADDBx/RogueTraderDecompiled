using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("4254059cf496468383e01f1c6efb982d")]
public class FixDuplicateVendorItems : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintSharedVendorTableReference[] m_Tables;

	private ReferenceArrayProxy<BlueprintSharedVendorTable> Tables
	{
		get
		{
			BlueprintReference<BlueprintSharedVendorTable>[] tables = m_Tables;
			return tables;
		}
	}

	public override string GetCaption()
	{
		if (m_Tables != null)
		{
			return "Fix Duplicates in Vendor items";
		}
		return "Action is not configured properly";
	}

	protected override void RunActionOverride()
	{
		foreach (BlueprintSharedVendorTable table2 in Tables)
		{
			SharedVendorTables.TableData table = Game.Instance.Player.SharedVendorTables.GetTable(table2);
			if (table == null)
			{
				break;
			}
			List<LootEntry> list = TempList.Get<LootEntry>();
			List<LootEntry> list2 = TempList.Get<LootEntry>();
			foreach (LootEntry entry in table.Entries)
			{
				if (list.HasItem((LootEntry i) => i.Item == entry.Item && i.IsDuplicate(entry)))
				{
					list2.Add(entry);
				}
				else
				{
					list.Add(entry);
				}
			}
			foreach (LootEntry item in list2)
			{
				table.Entries.Remove(item);
			}
		}
	}
}
