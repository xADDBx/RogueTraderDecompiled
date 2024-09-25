using System;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.TacticalCombat.ActionBar;

public class ActionBarSlotVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandStartHandler
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> IsApplying = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsCasting = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDisabled = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> CountText = new ReactiveProperty<string>();

	public readonly ReactiveCommand<int> SlotChanged = new ReactiveCommand<int>();

	public TooltipBaseTemplate TooltipTemplate;

	private readonly Action<ActionBarSlotVM> m_SetSlotSelected;

	public bool NeedKeybinding;

	public MechanicActionBarSlot MechanicActionBarSlot { get; private set; }

	public bool IsEmpty => MechanicActionBarSlot is MechanicActionBarSlotEmpty;

	public int Index { get; }

	public ActionBarSlotVM(MechanicActionBarSlot slot, int index, bool needKeybinding = false)
	{
		MechanicActionBarSlot = slot;
		Index = index;
		NeedKeybinding = needKeybinding;
		UpdateValues();
		AddDisposable(EventBus.Subscribe(this));
	}

	private void UpdateValues()
	{
		Icon.Value = MechanicActionBarSlot.GetIcon();
		IsApplying.Value = false;
		MechanicActionBarSlot mechanicActionBarSlot = MechanicActionBarSlot;
		if (!(mechanicActionBarSlot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility))
		{
			if (!(mechanicActionBarSlot is MechanicActionBarSlotActivableAbility mechanicActionBarSlotActivableAbility))
			{
				if (mechanicActionBarSlot is MechanicActionBarSlotEmpty)
				{
					TooltipTemplate = null;
				}
			}
			else
			{
				TooltipTemplate = new TooltipTemplateActivatableAbility(mechanicActionBarSlotActivableAbility.ActivatableAbility);
			}
		}
		else
		{
			TooltipTemplate = new TooltipTemplateAbility(mechanicActionBarSlotAbility.Ability);
			IsApplying.Value = mechanicActionBarSlotAbility.Ability.TargetAnchor != 0 && Game.Instance.SelectedAbilityHandler != null && Game.Instance.SelectedAbilityHandler.Ability == mechanicActionBarSlotAbility.Ability && MechanicActionBarSlot.GetResource() != 0;
		}
		IsCasting.Value = MechanicActionBarSlot.IsCasting();
		CountText.Value = MechanicActionBarSlot.GetCountText(MechanicActionBarSlot.GetResource());
		IsDisabled.Value = !MechanicActionBarSlot.IsPossibleActive;
	}

	public void SetMechanicSlot(MechanicActionBarSlot slot)
	{
		MechanicActionBarSlot = slot;
		UpdateValues();
		SlotChanged.Execute(Index);
	}

	public void OnClick()
	{
		MechanicActionBarSlot.OnClick();
		UpdateValues();
	}

	public void OnHover(bool value)
	{
		MechanicActionBarSlot.OnHover(value);
		UpdateValues();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor == MechanicActionBarSlot.Unit)
		{
			UpdateValues();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor == MechanicActionBarSlot.Unit)
		{
			UpdateValues();
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
