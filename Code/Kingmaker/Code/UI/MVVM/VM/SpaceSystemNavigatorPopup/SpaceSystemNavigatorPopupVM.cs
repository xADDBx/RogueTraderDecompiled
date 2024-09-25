using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.NavigatorResource;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceSystemNavigatorPopup;

public class SpaceSystemNavigatorPopupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISectorMapPassageChangeHandler, ISubscriber<ISectorMapPassageEntity>, ISubscriber, INavigatorResourceCountChangedHandler
{
	private readonly Action m_CloseAction;

	private readonly Action m_TravelAction;

	public readonly SectorMapObjectEntity SectorMapObject;

	public readonly BoolReactiveProperty WayOpenOrNot = new BoolReactiveProperty();

	public readonly IntReactiveProperty CreateWayCost = new IntReactiveProperty();

	public readonly IntReactiveProperty UpgradeWayCost = new IntReactiveProperty();

	public readonly IntReactiveProperty CurrentValueOfResources = new IntReactiveProperty();

	public readonly IntReactiveProperty ValueOfSkulls = new IntReactiveProperty();

	public readonly ReactiveProperty<bool> IsQuest = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsRumour = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsScannedFrom = new ReactiveProperty<bool>(initialValue: false);

	private Action m_OnLowerPassageEnded;

	public int CurrentValueOfResourcesChangeCount;

	public bool IsRightPassage;

	public SpaceSystemNavigatorPopupVM(Action closeAction, Action travelAction, SectorMapObjectEntity sectorMapObject, bool openOrNot)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_CloseAction = closeAction;
		m_TravelAction = travelAction;
		SectorMapObject = sectorMapObject;
		WayOpenOrNot.Value = openOrNot;
		CreateWayCost.Value = Game.Instance.Player.WarpTravelState.CreateNewPassageCost;
		UpgradeWayCost.Value = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.LowerPassageDifficultyCost;
		CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
		CheckQuests();
		CheckRumours();
		IsScannedFrom.Value = Game.Instance.SectorMapController.CurrentStarSystem.IsScannedFrom;
	}

	protected override void DisposeImplementation()
	{
	}

	private void CheckQuests()
	{
		List<QuestObjective> questsForSystem = UIUtilitySpaceQuests.GetQuestsForSystem(SectorMapObject.View);
		IsQuest.Value = !questsForSystem.Empty() && questsForSystem != null;
	}

	private void CheckRumours()
	{
		List<QuestObjective> rumoursForSystem = UIUtilitySpaceQuests.GetRumoursForSystem(SectorMapObject.View);
		IsRumour.Value = !rumoursForSystem.Empty() && rumoursForSystem != null;
	}

	public void SpaceSystemCreateWay()
	{
		Game.Instance.GameCommandQueue.CreateNewWarpRoute(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
	}

	public void HandleNewPassageCreated()
	{
		SectorMapPassageEntity sectorMapPassageEntity = Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
		if (EventInvokerExtensions.Entity as SectorMapPassageEntity == sectorMapPassageEntity)
		{
			WayOpenOrNot.Value = true;
			BlockPopups(state: false);
		}
	}

	public void SpaceSystemUpgradeWay(Action onEnded)
	{
		SectorMapPassageEntity sectorMapPassageEntity = Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
		Game.Instance.GameCommandQueue.LowerWarpRouteDifficulty(SectorMapObject, sectorMapPassageEntity.CurrentDifficulty - 1);
		m_OnLowerPassageEnded = onEnded;
	}

	public void HandlePassageLowerDifficulty()
	{
		SectorMapPassageEntity sectorMapPassageEntity = Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
		if (EventInvokerExtensions.Entity as SectorMapPassageEntity == sectorMapPassageEntity)
		{
			WayOpenOrNot.Value = true;
			BlockPopups(state: false);
			m_OnLowerPassageEnded?.Invoke();
		}
	}

	public void SpaceSystemTravelToSystem()
	{
		m_TravelAction?.Invoke();
		ClosePopups();
		BlockPopups(state: true);
	}

	public void SpaceSystemPopupClose()
	{
		m_CloseAction?.Invoke();
	}

	public void BlockPopups(bool state)
	{
		SectorMapOvertipsVM.Instance.BlockPopups(state);
	}

	public void ClosePopupsCanvas(bool state)
	{
		SectorMapOvertipsVM.Instance.ClosePopupsCanvas(state);
	}

	public void ClosePopups()
	{
		SectorMapOvertipsVM.Instance.ClosePopups();
	}

	public void NoMoneyReaction(int needMoneyCount)
	{
		SectorMapBottomHudVM.Instance.NoMoneyReaction(needMoneyCount);
	}

	public void SetDifficultySkulls()
	{
		SectorMapPassageEntity sectorMapPassageEntity = Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, SectorMapObject);
		ValueOfSkulls.Value = (int)sectorMapPassageEntity.CurrentDifficulty;
	}

	public void HandleChaneNavigatorResourceCount(int count)
	{
		if (count != 0)
		{
			CurrentValueOfResourcesChangeCount = count;
			CurrentValueOfResources.Value = Game.Instance.Player.WarpTravelState.NavigatorResource;
		}
	}
}
