using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.VM.SpaceSystemNavigatorPopup;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

public class OvertipEntitySystemVM : OvertipEntityVM, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, INavigatorResourceCountChangedHandler, INetRoleSetHandler
{
	public readonly SectorMapObjectEntity SectorMapObject;

	public readonly ReactiveProperty<OvertipSectorResourceBlockVM> SectorResourceBlockVM = new ReactiveProperty<OvertipSectorResourceBlockVM>();

	public readonly ReactiveProperty<bool> IsScanning = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTraveling = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsVisitAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsScanAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTravelAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTravelNewSectorAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsNotMainCharacter = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsTradeActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsDialogActive = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> RumourObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Colony> Colony = new ReactiveProperty<Colony>();

	private readonly ReactiveProperty<string> m_SystemName = new ReactiveProperty<string>();

	private readonly bool m_CanVisit;

	public readonly IntReactiveProperty CurrentValueOfResources = new IntReactiveProperty();

	public int CurrentValueOfResourcesChangeCount;

	public readonly ReactiveProperty<bool> IsCurrentSystem = new ReactiveProperty<bool>();

	public readonly ReactiveCommand HideSpaceSystemPopup = new ReactiveCommand();

	public readonly ReactiveProperty<SpaceSystemNavigationButtonsVM> SpaceSystemNavigationButtonsVM = new ReactiveProperty<SpaceSystemNavigationButtonsVM>();

	protected override bool UpdateEnabled
	{
		get
		{
			if (SectorMapObject.View.IsExploredOrHasQuests && SectorMapObject.View.IsVisible)
			{
				return SectorMapObject.IsAvailable;
			}
			return false;
		}
	}

	public bool IsExploredAndVisible => UpdateEnabled;

	private SectorMapController SectorMapController => Game.Instance.SectorMapController;

	protected override Vector3 GetEntityPosition()
	{
		return SectorMapObject.Position;
	}

	public OvertipEntitySystemVM(SectorMapObjectEntity sectorMapObjectEntity)
	{
		AddDisposable(EventBus.Subscribe(this));
		SectorMapObject = sectorMapObjectEntity;
		m_SystemName.Value = sectorMapObjectEntity.View.Name;
		if (Game.Instance.SectorMapController.CurrentStarSystem != null)
		{
			IsCurrentSystem.Value = sectorMapObjectEntity == Game.Instance.SectorMapController.CurrentStarSystem;
		}
		m_CanVisit = sectorMapObjectEntity.View.StarSystemToTransit != null;
		CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
	}

	protected override void DisposeImplementation()
	{
		DisposeResourceBlock();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		IsScanning.Value = SectorMapController.IsScanning;
		IsExplored.Value = SectorMapObject.View.IsExploredOrHasQuests;
		ReactiveProperty<bool> isDialogActive = IsDialogActive;
		BlueprintDialog blueprintDialog = Game.Instance.DialogController?.Dialog;
		isDialogActive.Value = blueprintDialog != null && (bool)blueprintDialog;
		if (Game.Instance.SectorMapController.CurrentStarSystem != null)
		{
			SectorMapObjectEntity currentStarSystem = Game.Instance.SectorMapController.CurrentStarSystem;
			bool flag = UINetUtility.IsControlMainCharacter();
			IsCurrentSystem.Value = SectorMapObject == currentStarSystem;
			IsVisitAvailable.Value = m_CanVisit && IsCurrentSystem.Value && flag;
			IsScanAvailable.Value = SectorMapObject == currentStarSystem && !SectorMapObject.IsScannedFrom && flag;
			IsTravelAvailable.Value = SectorMapObject != currentStarSystem && flag;
			IsTravelNewSectorAvailable.Value = Game.Instance.SectorMapController.GetStarSystemsToTravel().Contains(SectorMapObject.View) && flag;
			IsNotMainCharacter.Value = !flag;
		}
	}

	public bool CheckColonization()
	{
		Colony colony = Game.Instance.ColonizationController.GetColony(SectorMapObject.View);
		if (colony != null)
		{
			Colony.Value = colony;
		}
		return colony != null;
	}

	public bool CheckQuests()
	{
		List<QuestObjective> questsForSystem = UIUtilitySpaceQuests.GetQuestsForSystem(SectorMapObject.View);
		List<QuestObjective> questsForSpaceSystem = UIUtilitySpaceQuests.GetQuestsForSpaceSystem(SectorMapObject?.StarSystemArea);
		int num;
		if (questsForSystem == null || questsForSystem.Empty())
		{
			if (questsForSpaceSystem != null)
			{
				num = ((!questsForSpaceSystem.Empty()) ? 1 : 0);
				if (num != 0)
				{
					goto IL_0049;
				}
			}
			else
			{
				num = 0;
			}
			goto IL_006f;
		}
		num = 1;
		goto IL_0049;
		IL_0049:
		List<string> questsStringList = UIUtilitySpaceQuests.GetQuestsStringList(questsForSystem, questsForSpaceSystem);
		if (questsStringList.Any())
		{
			QuestObjectiveName.Value = string.Join(Environment.NewLine, questsStringList);
		}
		goto IL_006f;
		IL_006f:
		return (byte)num != 0;
	}

	public bool CheckRumours()
	{
		List<QuestObjective> rumoursForSystem = UIUtilitySpaceQuests.GetRumoursForSystem(SectorMapObject.View);
		if (rumoursForSystem != null && rumoursForSystem.Any())
		{
			List<string> list = rumoursForSystem.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list.Any())
			{
				RumourObjectiveName.Value = string.Join(Environment.NewLine, list);
			}
		}
		return rumoursForSystem?.Any() ?? false;
	}

	private void DisposeResourceBlock()
	{
		SectorResourceBlockVM.Value?.Dispose();
		SectorResourceBlockVM.Value = null;
	}

	private void TravelToSystem()
	{
		if (!CheckPingCoop() && SectorMapController.GetCurrentStarSystem() != SectorMapObject.View)
		{
			SectorMapController.WarpTravel(SectorMapObject.View);
		}
	}

	public void ShowSpaceSystemPopup()
	{
		if (Game.Instance.IsControllerMouse)
		{
			SectorMapOvertipsVM.Instance.ClosePopups();
		}
		CloseSpaceSystemPopup();
		SpaceSystemNavigationButtonsVM.Value = new SpaceSystemNavigationButtonsVM(CloseSpaceSystemPopup, TravelToSystem, SectorMapObject, IsTravelNewSectorAvailable.Value);
	}

	public void TravelToSystemImmediately()
	{
		TravelToSystem();
		SectorMapPassageEntity sectorMapPassageEntity = SectorMapController.FindPassageBetween(SectorMapController.CurrentStarSystem, SectorMapObject);
		if (sectorMapPassageEntity != null && sectorMapPassageEntity.IsExplored && !(SectorMapController.GetCurrentStarSystem() == SectorMapObject.View))
		{
			SectorMapOvertipsVM.Instance.ClosePopups();
			SectorMapOvertipsVM.Instance.BlockPopups(state: true);
		}
	}

	public void CloseSpaceSystemPopup()
	{
		SpaceSystemNavigationButtonsVM.Value?.Dispose();
		SpaceSystemNavigationButtonsVM.Value = null;
	}

	public void VisitSystem()
	{
		if (m_CanVisit)
		{
			if (Game.Instance.IsControllerMouse)
			{
				SectorMapOvertipsVM.Instance.ClosePopups();
			}
			SectorMapController.VisitStarSystem(SectorMapObject);
			EventBus.RaiseEvent(delegate(IGlobalMapSpaceSystemInformationWindowHandler h)
			{
				h.HandleHideSpaceSystemInformationWindow();
			});
			SectorMapOvertipsVM.Instance.BlockPopups(state: false);
		}
	}

	public void StartTrade()
	{
		if (!IsTradeActive.Value)
		{
			IsTradeActive.Value = true;
			OvertipSectorResourceBlockVM disposable = (SectorResourceBlockVM.Value = new OvertipSectorResourceBlockVM(SectorMapObject, DisposeResourceBlock));
			AddDisposable(disposable);
		}
	}

	public void StopTrade()
	{
		IsTradeActive.Value = false;
		DisposeResourceBlock();
	}

	public void HandleWarpTravelBeforeStart()
	{
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		IsTraveling.Value = true;
		EventBus.RaiseEvent(delegate(IGlobalMapSpaceSystemInformationWindowHandler h)
		{
			h.HandleHideSpaceSystemInformationWindow();
		});
	}

	public void HandleWarpTravelStopped()
	{
		IsTraveling.Value = false;
		if (Game.Instance.SectorMapController.CurrentStarSystem == SectorMapObject && Game.Instance.IsControllerMouse)
		{
			ShowVisitDialogBox(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject.IsVisited);
		}
	}

	public void ShowVisitDialogBox(SectorMapObjectEntity sectorMapObjectEntity, bool isVisited)
	{
		EventBus.RaiseEvent(delegate(IGlobalMapSpaceSystemInformationWindowHandler h)
		{
			h.HandleShowSpaceSystemInformationWindow();
		});
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
	}

	public void HandleWarpTravelPaused()
	{
		IsTraveling.Value = false;
	}

	public void HandleWarpTravelResumed()
	{
		IsTraveling.Value = true;
	}

	public void HandleChaneNavigatorResourceCount(int count)
	{
		if (SectorMapObject == Game.Instance.SectorMapController.CurrentStarSystem && count > 0)
		{
			CurrentValueOfResourcesChangeCount = count;
			CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
		}
	}

	public bool CheckPingCoop()
	{
		return PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(SectorMapObject);
		});
	}

	public void ChangeInformationInWindow()
	{
		EventBus.RaiseEvent(delegate(IGlobalMapInformationWindowsConsoleHandler h)
		{
			h.HandleChangeCurrentSystemInfoConsole(SectorMapObject);
		});
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		if (!UINetUtility.IsControlMainCharacter())
		{
			HideSpaceSystemPopup.Execute();
		}
	}
}
