using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[TypeId("af25a2d60e0c4fab980df67c71eeb9b7")]
public class FixItemInInventory : UnitUpgraderOnlyAction, IValidated
{
	[InfoBox("If both ToRemove and ToAdd specified but ToRemove is missing in unit's inventory then ToAdd will be ignored")]
	[SerializeField]
	private BlueprintItemReference m_ToRemove;

	[SerializeField]
	private BlueprintItemReference m_ToAdd;

	[SerializeField]
	[ShowIf("ShowTryToEquip")]
	private bool m_TryEquip;

	[CanBeNull]
	public BlueprintItem ToRemove => m_ToRemove;

	[CanBeNull]
	public BlueprintItem ToAdd => m_ToAdd;

	private bool ShowTryToEquip => ToAdd != null;

	public override string GetCaption()
	{
		if (ToAdd != null && ToRemove == null)
		{
			if (!m_TryEquip)
			{
				return $"Add item {ToAdd}";
			}
			return $"Add item {ToAdd} and try to equip";
		}
		if (ToAdd == null && ToRemove != null)
		{
			return $"Remove item {ToRemove}";
		}
		if (ToAdd != null && ToRemove != null)
		{
			if (!m_TryEquip)
			{
				return $"Replace item {ToRemove} on item {ToAdd}";
			}
			return $"Replace item {ToRemove} on item {ToAdd} and try to equip";
		}
		return "FixItem not configured properly";
	}

	protected override void RunActionOverride()
	{
		ItemSlot itemSlot = null;
		if (ToRemove != null)
		{
			ItemEntity itemEntity = base.Target.Inventory.Items.FirstItem((ItemEntity i) => i.Blueprint == ToRemove && (i.Wielder == null || i.Wielder == base.Target));
			if (itemEntity == null)
			{
				return;
			}
			itemSlot = itemEntity.HoldingSlot;
			base.Target.Inventory.Remove(itemEntity);
		}
		if (ToAdd == null)
		{
			return;
		}
		ItemEntity item = base.Target.Inventory.Add(ToAdd);
		if (!m_TryEquip)
		{
			return;
		}
		using (ContextData<ItemSlot.IgnoreLock>.Request())
		{
			if (itemSlot != null && itemSlot.CanInsertItem(item))
			{
				itemSlot.InsertItem(item);
				return;
			}
			foreach (ItemSlot equipmentSlot in base.Target.Body.EquipmentSlots)
			{
				if (equipmentSlot.MaybeItem == null && equipmentSlot.CanInsertItem(item))
				{
					equipmentSlot.InsertItem(item);
					break;
				}
			}
		}
	}

	public void Validate(ValidationContext context, int parentIndex)
	{
		if (ToRemove == null && ToAdd == null)
		{
			context.AddError("ToAdd and ToRemove are both null");
		}
	}
}
