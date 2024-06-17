using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;

public class ColonyManagementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonyManagementUIHandler, ISubscriber, IColonizationChronicleHandler, IGameModeHandler
{
	public readonly ReactiveProperty<bool> HasColonies = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsLockUIForDialog = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<ColonyManagementPageVM> ColonyManagementPage = new ReactiveProperty<ColonyManagementPageVM>();

	public readonly ReactiveProperty<Colony> CurrentColony = new ReactiveProperty<Colony>();

	public readonly ColonyManagementNavigationVM NavigationVM;

	public readonly LensSelectorVM Selector;

	private List<ColoniesState.ColonyData> Colonies => Game.Instance.Player.ColoniesState.Colonies;

	private bool HasColony => CurrentColony.Value != null;

	public ColonyManagementVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(NavigationVM = new ColonyManagementNavigationVM());
		AddDisposable(Selector = new LensSelectorVM());
		RefreshData();
	}

	protected override void DisposeImplementation()
	{
		NavigationVM.Clear();
	}

	public void OpenColonyProjects()
	{
		if (!HasColony)
		{
			PFLog.System.Error("ExplorationVM.OpenColonyProjects - can't open colony projects, colony is null!");
		}
		else
		{
			Game.Instance.GameCommandQueue.ColonyProjectsUIOpen(CurrentColony.Value.Blueprint.ToReference<BlueprintColonyReference>());
		}
	}

	public void HandleColonyManagementPage(Colony colony)
	{
		CurrentColony.Value = colony;
		if (colony != null && colony != ColonyManagementPage.Value?.Colony)
		{
			ColonyManagementPage.Value?.Dispose();
			NavigationVM.HandleColonyPage(colony);
			ColonyManagementPageVM disposable = (ColonyManagementPage.Value = new ColonyManagementPageVM(colony));
			AddDisposable(disposable);
			Game.Instance.CoroutinesController.InvokeInTicks(delegate
			{
				StartChronicle(colony);
			}, 1);
		}
	}

	public void HandleChronicleStarted(Colony colony, BlueprintDialog chronicle)
	{
	}

	public void HandleChronicleFinished(Colony colony, ColonyChronicle chronicle)
	{
		StartChronicle(colony);
	}

	private void RefreshData()
	{
		HasColonies.Value = Colonies.Count > 0;
		if (HasColonies.Value)
		{
			CurrentColony.Value = Colonies.FirstOrDefault()?.Colony;
			HandleColonyManagementPage(CurrentColony.Value);
		}
	}

	private void StartChronicle(Colony colony)
	{
		if (colony != null)
		{
			if (!colony.StartedChronicles.Any())
			{
				TryShowColonyRewards(colony);
				return;
			}
			UISounds.Instance.Sounds.SpaceColonization.ColonyEvent.Play();
			colony.StartChronicle(colony.StartedChronicles[0]);
		}
	}

	private void TryShowColonyRewards(Colony colony)
	{
		if (colony != null)
		{
			EventBus.RaiseEvent(delegate(IColonyManagementRewardsUIHandler h)
			{
				h.HandleColonyRewardsShow();
			});
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsLockUIForDialog.Value = gameMode == GameModeType.Dialog;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
