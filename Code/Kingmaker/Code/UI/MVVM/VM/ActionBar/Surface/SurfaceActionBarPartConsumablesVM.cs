using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Critters;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public class SurfaceActionBarPartConsumablesVM : SurfaceActionBarBasePartVM, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public readonly List<ActionBarSlotVM> Slots = new List<ActionBarSlotVM>();

	private const int MaxQuickSlotCount = 4;

	public SurfaceActionBarPartConsumablesVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void OnUnitChanged()
	{
		ClearSlots();
		if (Unit.Entity == null)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			UsableSlot quickSlot = Unit.Entity.Body.QuickSlots[i];
			if (!quickSlot.HasItem)
			{
				Slots.Add(new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = Unit
				}, i));
				continue;
			}
			if (quickSlot.Item.IsFamiliarItem())
			{
				Slots.Add(new ActionBarSlotVM(new MechanicActionBarSlotItem
				{
					Item = quickSlot.Item,
					Unit = Unit
				}, i));
				continue;
			}
			Ability ability = Unit.Entity.Abilities.Enumerable.FirstOrDefault((Ability a) => a.SourceItem == quickSlot.Item);
			if (ability != null)
			{
				Slots.Add(new ActionBarSlotVM(new MechanicActionBarSlotItem
				{
					Item = quickSlot.Item,
					Ability = ability,
					Unit = Unit
				}, i));
			}
			else
			{
				Slots.Add(new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = Unit
				}, i));
			}
		}
	}

	protected override void ClearSlots()
	{
		Slots.ForEach(delegate(ActionBarSlotVM slot)
		{
			slot.Dispose();
		});
		Slots.Clear();
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess && slot.Owner == Unit.Entity)
		{
			OnUnitChanged();
			UnitChanged.Execute();
		}
	}
}
