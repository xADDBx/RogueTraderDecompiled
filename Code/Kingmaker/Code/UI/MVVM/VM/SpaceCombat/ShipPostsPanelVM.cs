using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat;

public class ShipPostsPanelVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IClickMechanicActionBarSlotHandler, ISubscriber, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>
{
	public readonly ReactiveProperty<bool> IsPlayerTurn = new ReactiveProperty<bool>();

	public readonly List<ShipPostVM> Posts = new List<ShipPostVM>();

	public BoolReactiveProperty IsActive = new BoolReactiveProperty();

	public ShipPostsPanelVM()
	{
		List<Post> posts = Game.Instance.Player.PlayerShip.GetHull().Posts;
		for (int i = 0; i < posts.Count; i++)
		{
			Posts.Add(new ShipPostVM(i, posts[i]));
		}
		AddDisposable(EventBus.Subscribe(this));
		HandleTurnBasedModeSwitched(TurnController.IsInTurnBasedCombat());
	}

	protected override void DisposeImplementation()
	{
		Posts.ForEach(delegate(ShipPostVM postVm)
		{
			postVm.Dispose();
		});
		Posts.Clear();
	}

	public void Deactivate()
	{
		IsActive.Value = false;
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		Deactivate();
	}

	private void UpdatePlayerTurn(bool isTurnBased)
	{
		TurnController turnController = Game.Instance.TurnController;
		IsPlayerTurn.Value = isTurnBased && turnController.IsPlayerTurn && turnController.CurrentUnit == Game.Instance.Player.PlayerShip;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdatePlayerTurn(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		UpdatePlayerTurn(isTurnBased: true);
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		UpdatePlayerTurn(isTurnBased);
	}
}
