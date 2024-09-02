using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.CustomVideoPlayer;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.MainMenu;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame.Story;

public class NewGamePhaseStoryVM : NewGamePhaseBaseVm, INewGameChangeDlcHandler, ISubscriber, INewGameSwitchOnOffDlcHandler, IBugReportUIHandler
{
	private readonly ReactiveProperty<NewGamePhaseStoryScenarioEntityVM> m_SelectedEntity = new ReactiveProperty<NewGamePhaseStoryScenarioEntityVM>();

	public readonly SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM> SelectionGroup;

	private BlueprintCampaignReference m_CurrentCampaignReference;

	public readonly ReactiveCommand ChangeStory = new ReactiveCommand();

	public readonly BoolReactiveProperty DlcIsBought = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsAvailableToPurchase = new BoolReactiveProperty();

	public BlueprintDlc BlueprintDlc;

	public readonly BoolReactiveProperty DlcIsOn = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DownloadingInProgress = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsBoughtAndNotInstalled = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsRealConsole = new BoolReactiveProperty();

	public ReactiveProperty<Sprite> Art { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<VideoClip> Video { get; } = new ReactiveProperty<VideoClip>();


	public ReactiveProperty<string> SoundStart { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> SoundStop { get; } = new ReactiveProperty<string>();


	private ReactiveProperty<bool> IsDlcRequired { get; } = new ReactiveProperty<bool>();


	public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> CampaignName { get; } = new ReactiveProperty<string>();


	private BlueprintCampaign CurrentCampaign
	{
		get
		{
			if (m_CurrentCampaignReference != null && !m_CurrentCampaignReference.IsEmpty())
			{
				return m_CurrentCampaignReference.Get();
			}
			return null;
		}
		set
		{
			m_CurrentCampaignReference = value?.ToReference<BlueprintCampaignReference>();
		}
	}

	public CustomUIVideoPlayerVM CustomUIVideoPlayerVM { get; }

	public NewGamePhaseStoryVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		List<NewGamePhaseStoryScenarioEntityVM> list = new List<NewGamePhaseStoryScenarioEntityVM>();
		foreach (BlueprintCampaign campaign in BlueprintRoot.Instance.NewGameSettings.StoryCampaigns)
		{
			NewGamePhaseStoryScenarioEntityVM newGamePhaseStoryScenarioEntityVM = new NewGamePhaseStoryScenarioEntityVM(campaign, delegate
			{
				SetStory(campaign);
			});
			list.Add(newGamePhaseStoryScenarioEntityVM);
			AddDisposable(newGamePhaseStoryScenarioEntityVM);
		}
		SelectionGroup = new SelectionGroupRadioVM<NewGamePhaseStoryScenarioEntityVM>(list, m_SelectedEntity);
		AddDisposable(SelectionGroup);
		m_SelectedEntity.Value = list.First();
		AddDisposable(CustomUIVideoPlayerVM = new CustomUIVideoPlayerVM());
		IsRealConsole.Value = false;
		AddDisposable(EventBus.Subscribe(this));
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void DisposeImplementation()
	{
		SetStoreIconVisibility(visible: false);
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
		base.DisposeImplementation();
	}

	private void HandleOnRefreshDLC()
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(BlueprintDlc);
		DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && IsRealConsole.Value;
		DlcIsBought.Value = iDLCStatus.Purchased;
		DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && IsRealConsole.Value;
		SetStory(CurrentCampaign, BlueprintDlc);
	}

	private void SetStory(BlueprintCampaign campaign, BlueprintDlc blueprintDlc = null)
	{
		CampaignName.Value = campaign.Title;
		Video.Value = ((blueprintDlc != null) ? blueprintDlc.DefaultVideo : campaign.Video);
		SoundStart.Value = ((blueprintDlc != null) ? blueprintDlc.SoundStartEvent : campaign.SoundStartEvent);
		SoundStop.Value = ((blueprintDlc != null) ? blueprintDlc.SoundStopEvent : campaign.SoundStopEvent);
		Art.Value = ((blueprintDlc != null) ? blueprintDlc.GetKeyArt() : campaign.KeyArt);
		Description.Value = ((blueprintDlc != null && !string.IsNullOrWhiteSpace(blueprintDlc.GetDescription())) ? blueprintDlc.GetDescription() : ((string)campaign.Description));
		BlueprintDlc = blueprintDlc;
		BlueprintAreaPreset blueprintAreaPreset = campaign.StartGamePreset ?? GameStarter.MainPreset;
		bool flag = campaign.IsAvailable && blueprintAreaPreset != null;
		DlcIsBought.Value = blueprintDlc?.IsPurchased ?? flag;
		DlcIsAvailableToPurchase.Value = blueprintDlc == null || blueprintDlc.GetPurchaseState() != BlueprintDlc.DlcPurchaseState.ComingSoon;
		DlcIsOn.Value = blueprintDlc?.GetDlcSwitchOnOffState() ?? false;
		DownloadingInProgress.Value = blueprintDlc != null && blueprintDlc.GetDownloadState() == DownloadState.Loading && IsRealConsole.Value;
		DlcIsBoughtAndNotInstalled.Value = DlcIsBought.Value && blueprintDlc != null && blueprintDlc.GetDownloadState() == DownloadState.NotLoaded && IsRealConsole.Value;
		SetStoreIconVisibility(BlueprintDlc != null);
		if (CurrentCampaign == campaign)
		{
			ChangeStory.Execute();
			return;
		}
		MainMenuChargenUnits.Instance.DlcReward = campaign.DlcReward;
		IsDlcRequired.Value = !flag;
		base.IsNextButtonAvailable.Value = flag;
		CurrentCampaign = campaign;
		ChangeStory.Execute();
	}

	public void ShowInStore()
	{
		if (BlueprintDlc != null)
		{
			PFLog.UI.Log($"Open {BlueprintDlc} store");
			StoreManager.OpenShopFor(BlueprintDlc);
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxBase.BoxType.Message, null);
			return;
		}
		BlueprintDlc storyBlueprintDlc = GetStoryBlueprintDlc(CurrentCampaign);
		if (storyBlueprintDlc == null)
		{
			PFLog.UI.Log($"Story dlc of {CurrentCampaign} is null");
			return;
		}
		PFLog.UI.Log($"Open {CurrentCampaign.DlcReward} store");
		StoreManager.OpenShopFor(storyBlueprintDlc);
		UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxBase.BoxType.Message, null);
	}

	private BlueprintDlc GetStoryBlueprintDlc(BlueprintCampaign campaign)
	{
		return StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>().FirstOrDefault((BlueprintDlc pd) => pd.Rewards.FirstOrDefault((IBlueprintDlcReward r) => r is BlueprintDlcRewardCampaign blueprintDlcRewardCampaign && blueprintDlcRewardCampaign.Campaign == campaign) != null);
	}

	public void SwitchDlcOn()
	{
		BlueprintDlc?.SwitchDlcValue(!DlcIsOn.Value);
	}

	public override void OnNext()
	{
		if (base.IsNextButtonAvailable.Value)
		{
			SetStoreIconVisibility(visible: false);
			if (CurrentCampaign.AdditionalContentDlc.Any((BlueprintDlc dlc) => dlc.GetDownloadState() == DownloadState.Loading) || DownloadingInProgress.Value)
			{
				UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.NeedWaitAllDlcsDownload, DialogMessageBoxBase.BoxType.Message, null);
				return;
			}
			Game.NewGamePreset = GetPreset();
			base.OnNext();
		}
	}

	private BlueprintAreaPreset GetPreset()
	{
		return MainMenuChargenUnits.Instance.DlcReward?.Campaign?.StartGamePreset ?? GameStarter.MainPreset;
	}

	public void InstallDlc()
	{
		if (BlueprintDlc != null)
		{
			DownloadingInProgress.Value = true;
			DlcIsBoughtAndNotInstalled.Value = false;
			StoreManager.InstallDlc(BlueprintDlc);
		}
	}

	public void DeleteDlc()
	{
		if (BlueprintDlc == null)
		{
			return;
		}
		UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.AreYouSureDeleteDlc, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				DownloadingInProgress.Value = true;
				BlueprintDlc?.SwitchDlcValue(value: false);
				StoreManager.DeleteDlc(BlueprintDlc);
			}
		});
	}

	public void ResetVideo()
	{
		CustomUIVideoPlayerVM.ResetVideo();
	}

	public void HandleNewGameChangeDlc(BlueprintCampaign campaign, BlueprintDlc blueprintDlc)
	{
		SetStory(campaign, blueprintDlc);
	}

	public void HandleNewGameSwitchOnOffDlc(BlueprintDlc dlc, bool value)
	{
		if (dlc == BlueprintDlc)
		{
			DlcIsOn.Value = value;
		}
	}

	public void OnSelected()
	{
		SetStoreIconVisibility(BlueprintDlc != null);
	}

	private void SetStoreIconVisibility(bool visible)
	{
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
		SetStoreIconVisibility(visible: false);
	}

	public void HandleBugReportHide()
	{
		SetStoreIconVisibility(BlueprintDlc != null);
	}

	public void HandleUIElementFeature(string featureName)
	{
	}
}
