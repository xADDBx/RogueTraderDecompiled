using System.Collections.Generic;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public class SurfaceActionBarPartWeaponsVM : SurfaceActionBarBasePartVM, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, INetRoleSetHandler, IAbilityExecutionProcessHandler, ITurnStartHandler, IContinueTurnHandler, IInterruptTurnStartHandler, IUnitCommandEndHandler, IUnitCommandActHandler, IWarhammerAttackHandler, IUnitCommandStartHandler, IAbilityExecutionProcessClearedHandler, IInterruptTurnContinueHandler, IPlayerInputLockHandler
{
	public readonly ReactiveProperty<bool> CanSwitchSets = new ReactiveProperty<bool>();

	public readonly List<SurfaceActionBarPartWeaponSetVM> Sets = new List<SurfaceActionBarPartWeaponSetVM>();

	public readonly ReactiveProperty<SurfaceActionBarPartWeaponSetVM> CurrentSet = new ReactiveProperty<SurfaceActionBarPartWeaponSetVM>();

	public SurfaceActionBarPartWeaponsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(CurrentSet.Subscribe(delegate(SurfaceActionBarPartWeaponSetVM currentSet)
		{
			Sets.ForEach(delegate(SurfaceActionBarPartWeaponSetVM s)
			{
				s.UpdateIsCurrent(s == currentSet);
			});
		}));
	}

	protected override void OnUnitChanged()
	{
		RefreshCanSwitchSets();
		for (int i = 0; i < Unit.Entity.Body.HandsEquipmentSets.Count; i++)
		{
			if (i >= Sets.Count)
			{
				Sets.Add(new SurfaceActionBarPartWeaponSetVM());
			}
			int index = i;
			Sets[i].InitForUnit(index, Unit, Unit.Entity.Body.HandsEquipmentSets[i], delegate
			{
				SetCurrentEquipmentSet(index);
			});
			if (Unit.Entity.Body.CurrentHandsEquipmentSet == Unit.Entity.Body.HandsEquipmentSets[i])
			{
				CurrentSet.Value = Sets[i];
			}
		}
		if (Unit.Entity.Body.IsCurrentHandsEquipmentSetPolymorphed)
		{
			CurrentSet.Value = null;
		}
		for (int j = Unit.Entity.Body.HandsEquipmentSets.Count; j < Sets.Count; j++)
		{
			Sets[j].Dispose();
		}
		Sets.RemoveRange(Unit.Entity.Body.HandsEquipmentSets.Count, Sets.Count - Unit.Entity.Body.HandsEquipmentSets.Count);
	}

	protected override void ClearSlots()
	{
		Sets.ForEach(delegate(SurfaceActionBarPartWeaponSetVM setVm)
		{
			setVm.Dispose();
		});
		Sets.Clear();
	}

	private void SetCurrentEquipmentSet(int index)
	{
		if (CanSwitchSets.Value && (!Game.Instance.TurnController.TurnBasedModeActive || Game.Instance.TurnController.IsPlayerTurn) && Unit.Entity != null)
		{
			Game.Instance.GameCommandQueue.SwitchHandEquipment(Unit.Entity, index);
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot.Owner == Unit.Entity)
		{
			Sets.ForEach(delegate(SurfaceActionBarPartWeaponSetVM s)
			{
				s.UpdateSlots();
			});
		}
	}

	public void ChangeWeaponSet()
	{
		SetCurrentEquipmentSet((Unit.Entity.Body.CurrentHandEquipmentSetIndex == 0) ? 1 : 0);
	}

	void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet()
	{
		CurrentSet.Value = Sets.FirstOrDefault((SurfaceActionBarPartWeaponSetVM s) => s.HandSet == Unit.Entity.Body.CurrentHandsEquipmentSet);
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		RefreshCanSwitchSets();
	}

	private void RefreshCanSwitchSets()
	{
		CanSwitchSets.Value = Unit.Entity.IsDirectlyControllable() && !Unit.Entity.HasMechadendrites() && (!TurnController.IsInTurnBasedCombat() || !(Unit.Entity?.IsBusy ?? false));
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		RefreshCanSwitchSets();
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		RefreshCanSwitchSets();
	}

	public void HandleExecutionProcessCleared(AbilityExecutionContext context)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		RefreshCanSwitchSets();
	}

	public void HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		RefreshCanSwitchSets();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		RefreshCanSwitchSets();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		RefreshCanSwitchSets();
	}

	void IPlayerInputLockHandler.HandlePlayerInputLocked()
	{
		RefreshCanSwitchSets();
	}

	void IPlayerInputLockHandler.HandlePlayerInputUnlocked()
	{
		RefreshCanSwitchSets();
	}
}
