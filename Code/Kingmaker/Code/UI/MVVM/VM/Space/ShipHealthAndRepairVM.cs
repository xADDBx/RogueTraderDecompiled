using System;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.Scrap;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class ShipHealthAndRepairVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGameModeHandler, ISubscriber
{
	public readonly ReactiveProperty<int> MaxShipHealth = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CurrentShipHealth = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ScrapWeHave = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ScrapNeedForRepair = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanRepair = new ReactiveProperty<bool>();

	public readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public readonly Sprite PlayerShipSprite;

	public ShipHealthAndRepairVM(bool fromShipInventory = false, bool isLocked = false)
	{
		if (fromShipInventory)
		{
			ShouldShow.Value = true;
		}
		IsLocked.Value = isLocked;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		}));
		OnGameModeStart(Game.Instance?.CurrentMode);
		PlayerShipSprite = Game.Instance?.Player?.PlayerShip?.Blueprint?.Icon;
	}

	private void OnUpdate()
	{
		Player player = Game.Instance?.Player;
		PartHealth partHealth = player?.PlayerShip?.Health;
		if (partHealth != null)
		{
			MaxShipHealth.Value = partHealth.MaxHitPoints;
			CurrentShipHealth.Value = partHealth.HitPointsLeft;
			Scrap scrap = player.Scrap;
			if (scrap != null)
			{
				ScrapWeHave.Value = scrap;
				ScrapNeedForRepair.Value = ((CurrentShipHealth.Value < MaxShipHealth.Value) ? scrap.ScrapNeededForFullRepair : 0);
				CanRepair.Value = !Game.Instance.Player.IsInCombat && CurrentShipHealth.Value < MaxShipHealth.Value && ScrapWeHave.Value > 0 && !IsLocked.Value;
			}
		}
	}

	public void RepairShipFull()
	{
		Game.Instance?.Player?.Scrap?.RepairShipFull();
	}

	public void RepairShipForAllScrap()
	{
		Game.Instance?.Player?.Scrap?.RepairShipForAllScrap();
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		ShouldShow.Value = Game.Instance?.CurrentMode == GameModeType.StarSystem || RootUIContext.Instance.IsShipInventoryShown;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		ShouldShow.Value = Game.Instance?.CurrentMode == GameModeType.StarSystem || RootUIContext.Instance.IsShipInventoryShown;
	}
}
