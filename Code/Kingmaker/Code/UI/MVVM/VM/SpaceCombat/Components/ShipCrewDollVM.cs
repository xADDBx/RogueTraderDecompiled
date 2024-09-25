using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI;
using Owlcat.Runtime.UI.MVVM;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;

public class ShipCrewDollVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<ShipCrewDollModuleVM> Modules = new List<ShipCrewDollModuleVM>();

	public ShipCrewDollVM()
	{
		foreach (ShipModuleType value in Enum.GetValues(typeof(ShipModuleType)))
		{
			ShipCrewDollModuleVM shipCrewDollModuleVM = new ShipCrewDollModuleVM(value, SelectShip, Game.Instance.Player.PlayerShip);
			Modules.Add(shipCrewDollModuleVM);
			AddDisposable(shipCrewDollModuleVM);
		}
	}

	protected override void DisposeImplementation()
	{
	}

	private void SelectShip()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		UIAccess.SelectionManager.SelectUnit(playerShip.View);
		Game.Instance.CameraController?.Follower?.ScrollTo(playerShip);
	}
}
