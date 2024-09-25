using System;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class InsertableLootSlotVM : ItemSlotVM
{
	public readonly ReactiveProperty<bool> CanInsert = new ReactiveProperty<bool>();

	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	public override bool CanTransfer
	{
		get
		{
			if (CanInsert.Value)
			{
				return base.HasItem;
			}
			return false;
		}
	}

	public InsertableLootSlotVM(ItemEntity item, int index, ISlotsGroupVM group = null, Func<ItemEntity, bool> canInsertItem = null)
		: base(item, index, group)
	{
		m_CanInsertItem = canInsertItem;
		UpdateCanInsert();
	}

	protected override void ItemChangedHandler(ItemEntity item)
	{
		base.ItemChangedHandler(item);
		UpdateCanInsert();
	}

	public void UpdateCanInsert()
	{
		ItemEntity value = Item.Value;
		CanInsert.Value = !base.HasItem || (m_CanInsertItem?.Invoke(value) ?? true);
	}
}
