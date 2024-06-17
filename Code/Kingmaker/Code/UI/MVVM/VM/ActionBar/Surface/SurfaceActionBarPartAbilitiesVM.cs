using System;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public class SurfaceActionBarPartAbilitiesVM : SurfaceActionBarBasePartVM, IActionBarPartAbilitiesHandler, ISubscriber, IClickMechanicActionBarSlotHandler, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, IEntityLostFactHandler, IEntitySubscriber
{
	public readonly bool IsInCharScreen;

	public readonly AutoDisposingList<ActionBarSlotVM> Slots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly ReactiveCommand SlotCountChanged = new ReactiveCommand();

	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty();

	public readonly ReactiveProperty<AbilitySelectorWindowVM> AbilitySelectorWindowVM = new ReactiveProperty<AbilitySelectorWindowVM>();

	public readonly BoolReactiveProperty MoveAbilityMode = new BoolReactiveProperty();

	private IDisposable m_OnAbilitiesChangedDelay;

	public int RowIndex
	{
		get
		{
			return Unit.Entity?.UISettings.SlotRowIndexConsole ?? 0;
		}
		set
		{
			if (Unit.Entity != null)
			{
				Unit.Entity.UISettings.SlotRowIndexConsole = value;
			}
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return Unit.Entity;
	}

	public SurfaceActionBarPartAbilitiesVM(bool isInCharScreen)
	{
		IsInCharScreen = isInCharScreen;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		AbilitySelectorWindowVM.Value?.Dispose();
	}

	protected override void OnUnitChanged()
	{
		ClearSlots();
		FillSlots(20);
		AddOrRemoveEmptyRowIfNeeded();
		for (int i = 0; i < Unit.Entity.UISettings.Slots.Count; i++)
		{
			ActionBarSlotVM item = new ActionBarSlotVM(Unit.Entity.UISettings.GetSlot(i, Unit.Entity), i, IsInCharScreen, MoveAbilityMode);
			Slots.Add(item);
		}
	}

	private void UpdateSlots()
	{
		if (Unit.Entity == null)
		{
			return;
		}
		AddOrRemoveEmptyRowIfNeeded();
		int count = Unit.Entity.UISettings.Slots.Count;
		bool flag = Slots.Count != count;
		for (int i = 0; i < count; i++)
		{
			MechanicActionBarSlot slot = Unit.Entity.UISettings.GetSlot(i, Unit.Entity);
			if (i < Slots.Count)
			{
				Slots[i].SetMechanicSlot(slot);
				continue;
			}
			ActionBarSlotVM item = new ActionBarSlotVM(slot, i, IsInCharScreen, MoveAbilityMode);
			Slots.Add(item);
		}
		if (Slots.Count > count)
		{
			Slots.RemoveRangeAndDispose(count, Slots.Count - count);
		}
		if (flag)
		{
			SlotCountChanged.Execute();
		}
	}

	protected override void ClearSlots()
	{
		Slots.Clear();
	}

	private void FillSlots(int count)
	{
		Unit.Entity.UISettings.GetSlot(count - 1, Unit.Entity);
	}

	private void AddOrRemoveEmptyRowIfNeeded()
	{
		int num = Unit.Entity.UISettings.Slots.Count / 10;
		int num2 = Mathf.Max(Unit.Entity.UISettings.Slots.FindLastIndex((MechanicActionBarSlot s) => s is MechanicActionBarSlotAbility) / 10, 0);
		int num3 = num - num2;
		if (num3 > 1)
		{
			if (num3 != 2)
			{
				Unit.Entity.UISettings.RemoveFromIndexToEnd((num2 + 2) * 10);
			}
		}
		else
		{
			FillSlots((num + 1) * 10);
		}
	}

	public void MoveSlot(Ability sourceAbility, int targetIndex)
	{
		if (Unit.Entity == null || sourceAbility.Owner != Unit.Entity)
		{
			return;
		}
		MechanicActionBarSlot slot = Unit.Entity.UISettings.GetSlot(targetIndex, Unit.Entity);
		if (slot is MechanicActionBarSlotAbility || slot is MechanicActionBarSlotEmpty)
		{
			if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.Ability == sourceAbility.Data)
			{
				UpdateSlots();
				return;
			}
			Unit.Entity.UISettings.SetSlot(Unit.Entity, sourceAbility, targetIndex);
			UpdateSlots();
		}
	}

	public void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex)
	{
		if (Unit.Entity == null || sourceSlot.Unit != Unit.Entity)
		{
			return;
		}
		MechanicActionBarSlot slot = Unit.Entity.UISettings.GetSlot(targetIndex, Unit.Entity);
		if (slot is MechanicActionBarSlotAbility || slot is MechanicActionBarSlotEmpty)
		{
			if (slot == sourceSlot)
			{
				UpdateSlots();
				return;
			}
			Unit.Entity.UISettings.SetSlot(sourceSlot, targetIndex);
			Unit.Entity.UISettings.SetSlot(slot, sourceIndex);
			UpdateSlots();
		}
	}

	public void DeleteSlot(int sourceIndex)
	{
		if (Unit.Entity != null)
		{
			Unit.Entity.UISettings.RemoveSlot(sourceIndex);
			UpdateSlots();
		}
	}

	public void ChooseAbilityToSlot(int targetIndex)
	{
		if (RootUIContext.Instance.CurrentServiceWindow == ServiceWindowsType.CharacterInfo)
		{
			return;
		}
		AbilitySelectorWindowVM.Value?.Dispose();
		AbilitySelectorWindowVM.Value = new AbilitySelectorWindowVM(delegate(CharInfoFeatureVM vm)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(vm.Ability, targetIndex);
			});
		}, HideSelectionWindow, Unit);
	}

	private void HideSelectionWindow()
	{
		AbilitySelectorWindowVM.Value?.Dispose();
		AbilitySelectorWindowVM.Value = null;
	}

	public void SetMoveAbilityMode(bool on)
	{
		MoveAbilityMode.Value = on;
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		IsActive.Value = false;
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		OnAbilitiesChanged();
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		OnAbilitiesChanged();
	}

	private void OnAbilitiesChanged()
	{
		if (Unit.Entity == null)
		{
			return;
		}
		m_OnAbilitiesChangedDelay?.Dispose();
		m_OnAbilitiesChangedDelay = DelayedInvoker.InvokeInFrames(delegate
		{
			if (!base.IsDisposed)
			{
				Unit.Entity?.UISettings.TryToInitialize();
				UpdateSlots();
			}
		}, 1);
	}
}
