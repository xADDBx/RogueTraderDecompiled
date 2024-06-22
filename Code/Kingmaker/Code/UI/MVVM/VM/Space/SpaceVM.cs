using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Space;

public class SpaceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, IStarShipLandingHandler, IGameModeHandler
{
	public readonly SpaceStaticPartVM StaticPartVM;

	public readonly SpaceDynamicPartVM DynamicPartVM;

	public readonly ReactiveProperty<SpaceMode> SpaceMode = new ReactiveProperty<SpaceMode>();

	public bool IsInQuestNotification => QuestNotificatorVM.Instance?.IsShowUp.Value ?? false;

	public SpaceVM()
	{
		AddDisposable(StaticPartVM = new SpaceStaticPartVM());
		AddDisposable(DynamicPartVM = new SpaceDynamicPartVM());
		if (TurnController.IsInTurnBasedCombat())
		{
			SpaceMode.Value = Kingmaker.Code.UI.MVVM.VM.Space.SpaceMode.SpaceCombat;
		}
		else
		{
			SetSpaceMode(Game.Instance.CurrentMode);
		}
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetSpaceMode(GameModeType gameMode)
	{
		if (gameMode == GameModeType.GlobalMap)
		{
			SpaceMode.Value = Kingmaker.Code.UI.MVVM.VM.Space.SpaceMode.GlobalMap;
		}
		if (gameMode == GameModeType.StarSystem)
		{
			SpaceMode.Value = Kingmaker.Code.UI.MVVM.VM.Space.SpaceMode.SystemMap;
		}
	}

	private void TryHandleDestinationArrival(StarSystemObjectView sso)
	{
		if (RootUIContext.Instance.CurrentServiceWindow != 0)
		{
			if (UINetUtility.IsControlMainCharacter())
			{
				return;
			}
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleCloseAll();
			});
		}
		BlueprintMultiEntrance multiEntrance = sso.Blueprint?.Get()?.GetComponent<MultiEntranceObject>()?.Entrance?.Get();
		if (multiEntrance != null)
		{
			EventBus.RaiseEvent(delegate(IMultiEntranceHandler h)
			{
				h.HandleMultiEntrance(multiEntrance);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IExplorationUIHandler h)
			{
				h.OpenExplorationScreen(sso);
			});
		}
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			SpaceMode.Value = Kingmaker.Code.UI.MVVM.VM.Space.SpaceMode.SpaceCombat;
		}
	}

	void ITurnBasedModeResumeHandler.HandleTurnBasedModeResumed()
	{
		SpaceMode.Value = Kingmaker.Code.UI.MVVM.VM.Space.SpaceMode.SpaceCombat;
	}

	public void HandleStarShipLanded(StarSystemObjectView sso)
	{
		TryHandleDestinationArrival(sso);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		SetSpaceMode(gameMode);
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OpenJournal()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenJournal();
		});
	}

	public void QuestNotificationForceClose()
	{
		QuestNotificatorVM.Instance.ForceClose();
	}
}
