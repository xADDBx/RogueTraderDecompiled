using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.TwitchDrops;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class LootContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IExplorationCargoHandler, IGameModeHandler, IAddCargoActionHandler, ITwitchDropsRewardsUIHandler
{
	public enum LootWindowMode
	{
		Short,
		ShortUnit,
		ZoneExit,
		PlayerChest,
		StandardChest,
		OneSlot
	}

	public readonly ReactiveProperty<LootVM> LootVM = new ReactiveProperty<LootVM>();

	public readonly ReactiveProperty<CargoRewardsVM> CargoVM = new ReactiveProperty<CargoRewardsVM>();

	public readonly ReactiveProperty<TwitchDropsRewardsVM> TwitchDropsRewardsVM = new ReactiveProperty<TwitchDropsRewardsVM>();

	private bool m_IsInitializing;

	public bool IsShown => LootVM.Value != null;

	public LootContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeLoot();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
		if (EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			if (IsShown)
			{
				DisposeLoot();
			}
			LootWindowMode mode = containerType switch
			{
				LootContainerType.OneSlot => LootWindowMode.OneSlot, 
				LootContainerType.PlayerChest => LootWindowMode.PlayerChest, 
				LootContainerType.Unit => LootWindowMode.ShortUnit, 
				_ => (objects.Length <= 1 && containerType != LootContainerType.Environment && containerType != 0) ? LootWindowMode.StandardChest : LootWindowMode.Short, 
			};
			closeCallback = (Action)Delegate.Combine(closeCallback, new Action(DisposeLoot));
			LootVM disposable = (LootVM.Value = new LootVM(mode, objects, closeCallback));
			AddDisposable(disposable);
			DisposeLootIfNeeded();
		}
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
		if (EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			if (IsShown)
			{
				DisposeLoot();
			}
			if (containerType == LootContainerType.StarSystemObject)
			{
				LootWindowMode mode = LootWindowMode.ShortUnit;
				closeCallback = (Action)Delegate.Combine(closeCallback, new Action(DisposeLoot));
				LootVM disposable = (LootVM.Value = new LootVM(mode, objects, containerType, closeCallback, skillCheckResult));
				AddDisposable(disposable);
				DisposeLootIfNeeded();
			}
		}
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		if (!EventInvokerExtensions.BaseUnitEntity.IsMyNetRole())
		{
			return;
		}
		if (IsShown)
		{
			if (LootVM.Value.Mode == LootWindowMode.ZoneExit)
			{
				return;
			}
			DisposeLoot();
		}
		BaseUnitEntity invokerEntity = EventInvokerExtensions.BaseUnitEntity;
		LootVM disposable = (LootVM.Value = new LootVM(LootWindowMode.ZoneExit, MassLootHelper.GetMassLootFromCurrentArea(), delegate
		{
			bool isPlayerCommand = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
			AreaTransitionGroupCommand.ExecuteTransition(areaTransition, isPlayerCommand, invokerEntity);
		}, DisposeLoot));
		AddDisposable(disposable);
		DisposeLootIfNeeded();
	}

	public void HandlePointOfInterestCargoInteraction(PointOfInterestCargo pointOfInterest)
	{
		if (Game.Instance.Player.MainCharacterEntity.IsMyNetRole())
		{
			List<BlueprintCargo> explorationCargo = pointOfInterest.Blueprint.ExplorationCargo;
			if (explorationCargo != null)
			{
				CargoRewardsVM disposable = (CargoVM.Value = new CargoRewardsVM(explorationCargo, DisposeCargo));
				AddDisposable(disposable);
			}
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (IsShown && (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.Dialog))
		{
			DisposeLoot();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private void DisposeLootIfNeeded()
	{
		LootVM value = LootVM.Value;
		if (value != null && value.IsDisposed)
		{
			LootVM.Value = null;
		}
	}

	private void DisposeLoot()
	{
		LootVM.Value?.Dispose();
		LootVM.Value = null;
	}

	private void DisposeCargo()
	{
		CargoVM.Value?.Dispose();
		CargoVM.Value = null;
		EventBus.RaiseEvent(delegate(ICargoRewardsUIHandler h)
		{
			h.HandleCargoRewardsHide();
		});
	}

	void IAddCargoActionHandler.HandleAddCargoAction(List<CargoEntity> cargoEntities)
	{
		if (!cargoEntities.Any())
		{
			return;
		}
		if (CargoVM.Value != null)
		{
			CargoVM.Value.AddCargo(cargoEntities);
			return;
		}
		CargoRewardsVM disposable = (CargoVM.Value = new CargoRewardsVM(cargoEntities, DisposeCargo));
		AddDisposable(disposable);
		EventBus.RaiseEvent(delegate(ICargoRewardsUIHandler h)
		{
			h.HandleCargoRewardsShow();
		});
	}

	public void HandleItemRewardsShow()
	{
		TwitchDropsRewardsVM disposable = (TwitchDropsRewardsVM.Value = new TwitchDropsRewardsVM(DisposeItems));
		AddDisposable(disposable);
	}

	private void DisposeItems()
	{
		TwitchDropsRewardsVM.Value?.Dispose();
		TwitchDropsRewardsVM.Value = null;
	}
}
