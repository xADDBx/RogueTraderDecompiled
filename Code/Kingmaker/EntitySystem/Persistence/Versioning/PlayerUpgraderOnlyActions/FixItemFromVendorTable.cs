using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("985e067863e44b01a2a495e951cccdff")]
public class FixItemFromVendorTable : PlayerUpgraderOnlyAction, IValidated
{
	[SerializeField]
	private BlueprintSharedVendorTableReference m_VendorTable;

	[InfoBox("If both ToRemove and ToAdd specified but ToRemove is missing in VendorTable then ToAdd will be ignored")]
	[SerializeField]
	private BlueprintItemReference m_ToRemove;

	[SerializeField]
	private LootEntry m_ToAdd;

	[CanBeNull]
	public BlueprintItem ToRemove => m_ToRemove;

	[CanBeNull]
	public LootEntry ToAdd => m_ToAdd;

	public BlueprintSharedVendorTable VendorTable => m_VendorTable;

	public override string GetCaption()
	{
		if (ToAdd != null && ToRemove == null)
		{
			return $"Add item {ToAdd.Item} to {VendorTable.name}";
		}
		if (ToAdd == null && ToRemove != null)
		{
			return $"Remove item {ToRemove} from {VendorTable.name}";
		}
		if (ToAdd != null && ToRemove != null)
		{
			return $"Replace item {ToRemove} on item {ToAdd.Item}";
		}
		return "FixVendorItem not configured properly";
	}

	protected override void RunActionOverride()
	{
		SharedVendorTables.TableData table = Game.Instance.Player.SharedVendorTables.GetTable(VendorTable);
		if (table == null)
		{
			return;
		}
		if (ToRemove != null)
		{
			LootEntry lootEntry = table.Entries.FirstItem((LootEntry i) => i.Item == ToRemove);
			if (lootEntry == null)
			{
				return;
			}
			table.Entries.Remove(lootEntry);
		}
		if (ToAdd != null)
		{
			table.Entries.Add(ToAdd);
		}
	}

	public void Validate(ValidationContext context, int parentIndex)
	{
		if (VendorTable == null)
		{
			context.AddError("Vendor Table is empty");
		}
		if (ToRemove == null && ToAdd == null)
		{
			context.AddError("ToAdd and ToRemove are both empty");
		}
	}
}
