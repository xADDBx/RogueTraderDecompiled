using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.CustomVideoPlayer;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;

public class DlcManagerTabDlcsVM : DlcManagerTabBaseVM
{
	public readonly ReactiveProperty<DlcManagerDlcEntityVM> SelectedEntity = new ReactiveProperty<DlcManagerDlcEntityVM>();

	public readonly SelectionGroupRadioVM<DlcManagerDlcEntityVM> SelectionGroup;

	public readonly ReactiveCommand ChangeStory = new ReactiveCommand();

	private BlueprintDlc m_CurrentDlc;

	private ReactiveCollection<DlcManagerDlcEntityVM> m_DLCsCollection = new ReactiveCollection<DlcManagerDlcEntityVM>();

	public readonly BoolReactiveProperty DlcIsBought = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsAvailableToPurchase = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DownloadingInProgress = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsBoughtAndNotInstalled = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsRealConsole = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsEditionDlc = new BoolReactiveProperty();

	public ReactiveProperty<Sprite> Art { get; } = new ReactiveProperty<Sprite>();


	public ReactiveProperty<VideoClip> Video { get; } = new ReactiveProperty<VideoClip>();


	public ReactiveProperty<string> SoundStart { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> SoundStop { get; } = new ReactiveProperty<string>();


	public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();


	public CustomUIVideoPlayerVM CustomUIVideoPlayerVM { get; }

	public DlcManagerTabDlcsVM()
	{
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerDlcEntityVM>(m_DLCsCollection, SelectedEntity);
		AddDisposable(SelectionGroup);
		UpdateDLCs();
		AddDisposable(CustomUIVideoPlayerVM = new CustomUIVideoPlayerVM());
		IsRealConsole.Value = false;
		AddDisposable(EventBus.Subscribe(this));
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
	}

	private void UpdateDLCs()
	{
		List<DlcManagerDlcEntityVM> list = new List<DlcManagerDlcEntityVM>();
		foreach (IBlueprintDlc item in from dlc in StoreManager.GetPurchasableDLCs()
			orderby dlc.DlcType
			select dlc)
		{
			BlueprintDlc dlc = item as BlueprintDlc;
			if (dlc == null || (!dlc.HideWhoNotBuyDlc && !dlc.HideDlcForAll) || 1 == 0)
			{
				DlcManagerDlcEntityVM dlcManagerDlcEntityVM = new DlcManagerDlcEntityVM(dlc, delegate
				{
					SetDlc(dlc);
				});
				list.Add(dlcManagerDlcEntityVM);
				AddDisposable(dlcManagerDlcEntityVM);
			}
		}
		if (list.Count == m_DLCsCollection.Count)
		{
			return;
		}
		m_DLCsCollection.Clear();
		foreach (DlcManagerDlcEntityVM item2 in list)
		{
			m_DLCsCollection.Add(item2);
		}
		ReactiveProperty<DlcManagerDlcEntityVM> selectedEntity = SelectedEntity;
		if (selectedEntity.Value == null)
		{
			DlcManagerDlcEntityVM dlcManagerDlcEntityVM3 = (selectedEntity.Value = m_DLCsCollection.FirstOrDefault());
		}
	}

	private void HandleOnRefreshDLC()
	{
		UpdateDLCs();
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(m_CurrentDlc);
		DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && IsRealConsole.Value;
		DlcIsBought.Value = iDLCStatus.Purchased;
		DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && IsRealConsole.Value;
		SetDlc(m_CurrentDlc);
	}

	private void SetDlc(BlueprintDlc blueprintDlc)
	{
		if (blueprintDlc != null)
		{
			m_CurrentDlc = blueprintDlc;
			Video.Value = blueprintDlc.DefaultVideo;
			SoundStart.Value = blueprintDlc.SoundStartEvent;
			SoundStop.Value = blueprintDlc.SoundStopEvent;
			Art.Value = blueprintDlc.GetKeyArt();
			Description.Value = blueprintDlc.GetDescription();
			bool isPurchased = blueprintDlc.IsPurchased;
			DlcIsBought.Value = isPurchased;
			DlcIsAvailableToPurchase.Value = blueprintDlc.GetPurchaseState() != BlueprintDlc.DlcPurchaseState.ComingSoon;
			DownloadState downloadState = blueprintDlc.GetDownloadState();
			DownloadingInProgress.Value = downloadState == DownloadState.Loading && IsRealConsole.Value;
			DlcIsBoughtAndNotInstalled.Value = isPurchased && downloadState == DownloadState.NotLoaded && IsRealConsole.Value;
			BoolReactiveProperty isEditionDlc = IsEditionDlc;
			DlcTypeEnum dlcType = blueprintDlc.DlcType;
			isEditionDlc.Value = dlcType == DlcTypeEnum.CosmeticDlc || dlcType == DlcTypeEnum.PromotionalDlc;
			ChangeStory.Execute();
		}
	}

	public void ShowInStore()
	{
		if (m_CurrentDlc != null)
		{
			PFLog.UI.Log($"Open {m_CurrentDlc} store");
			StoreManager.OpenShopFor(m_CurrentDlc);
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.NeedRestartAfterPurchase, DialogMessageBoxBase.BoxType.Message, null);
		}
	}

	public void InstallDlc()
	{
		if (m_CurrentDlc != null)
		{
			DownloadingInProgress.Value = true;
			DlcIsBoughtAndNotInstalled.Value = false;
			if (SelectedEntity?.Value != null)
			{
				SelectedEntity.Value.DownloadingInProgress.Value = true;
				SelectedEntity.Value.DlcIsBoughtAndNotInstalled.Value = false;
			}
			StoreManager.InstallDlc(m_CurrentDlc);
		}
	}

	public void DeleteDlc()
	{
		if (m_CurrentDlc == null)
		{
			return;
		}
		UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.AreYouSureDeleteDlc, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				DownloadingInProgress.Value = true;
				if (SelectedEntity?.Value != null)
				{
					SelectedEntity.Value.DownloadingInProgress.Value = true;
				}
				StoreManager.DeleteDlc(m_CurrentDlc);
			}
		});
	}

	public void ResetVideo()
	{
		CustomUIVideoPlayerVM.ResetVideo();
	}
}
