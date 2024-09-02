using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;

public class DlcManagerDlcEntityVM : SelectionGroupEntityVM
{
	public readonly BlueprintDlc BlueprintDlc;

	private Action m_SetDlc;

	public readonly string Title;

	public readonly Sprite Art;

	public readonly DlcTypeEnum DlcType;

	private readonly string ICH_HABE_ES_GESEHEN_PREF_KEY;

	public readonly BoolReactiveProperty SawThisDlc = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DownloadingInProgress = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsBoughtAndNotInstalled = new BoolReactiveProperty();

	public readonly BoolReactiveProperty DlcIsInstalled = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsDlcCanBeDeleted = new BoolReactiveProperty();

	private readonly bool m_IsRealConsole;

	private bool CurrentSawValue => PlayerPrefs.GetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 0) == 1;

	public DlcManagerDlcEntityVM(BlueprintDlc blueprintDlc, Action setDlc)
		: base(allowSwitchOff: false)
	{
		m_IsRealConsole = false;
		BlueprintDlc = blueprintDlc;
		m_SetDlc = setDlc;
		Title = blueprintDlc.GetDlcName();
		Art = ((blueprintDlc.DlcItemArtLink != null) ? blueprintDlc.DlcItemArtLink : UIConfig.Instance.DlcEntityKeyArt);
		DlcType = blueprintDlc.DlcType;
		DownloadState downloadState = blueprintDlc.GetDownloadState();
		DownloadingInProgress.Value = downloadState == DownloadState.Loading && m_IsRealConsole;
		DlcIsBoughtAndNotInstalled.Value = blueprintDlc.IsPurchased && downloadState == DownloadState.NotLoaded && m_IsRealConsole;
		DlcIsInstalled.Value = blueprintDlc.IsPurchased && downloadState == DownloadState.Loaded && m_IsRealConsole;
		IsDlcCanBeDeleted.Value = downloadState == DownloadState.Loaded && blueprintDlc.DlcType == DlcTypeEnum.AdditionalContentDlc;
		ICH_HABE_ES_GESEHEN_PREF_KEY = "DLCMANAGER_I_SAW_" + blueprintDlc.name;
		SawThisDlc.Value = CurrentSawValue;
		AddDisposable(EventBus.Subscribe(this));
		StoreManager.OnRefreshDLC += HandleOnRefreshDLC;
	}

	protected override void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= HandleOnRefreshDLC;
		m_SetDlc = null;
	}

	private void HandleOnRefreshDLC()
	{
		IDLCStatus iDLCStatus = StoreManager.DLCCache.Get(BlueprintDlc);
		DownloadingInProgress.Value = iDLCStatus.DownloadState == DownloadState.Loading && m_IsRealConsole;
		DlcIsBoughtAndNotInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.NotLoaded && m_IsRealConsole;
		DlcIsInstalled.Value = iDLCStatus.Purchased && iDLCStatus.DownloadState == DownloadState.Loaded && m_IsRealConsole;
		IsDlcCanBeDeleted.Value = iDLCStatus.DownloadState == DownloadState.Loaded && BlueprintDlc.DlcType == DlcTypeEnum.AdditionalContentDlc;
	}

	public void SelectMe()
	{
		DoSelectMe();
	}

	protected override void DoSelectMe()
	{
		m_SetDlc?.Invoke();
		if (!CurrentSawValue)
		{
			PlayerPrefs.SetInt(ICH_HABE_ES_GESEHEN_PREF_KEY, 1);
			PlayerPrefs.Save();
			if (SawThisDlc.Value != CurrentSawValue)
			{
				SawThisDlc.Value = CurrentSawValue;
			}
		}
	}
}
