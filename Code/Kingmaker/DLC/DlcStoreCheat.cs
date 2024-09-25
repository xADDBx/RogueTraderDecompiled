using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("fd601a0246034ca38414c127dbcc65ea")]
public class DlcStoreCheat : DlcStore
{
	[SerializeField]
	private string m_TestShopLink = "https://roguetrader.owlcat.games/";

	[SerializeField]
	[Tooltip("Is the DLC available in editor playmode")]
	private bool m_IsAvailableInEditor;

	[SerializeField]
	[Tooltip("Is the DLC available in development build")]
	private bool m_IsAvailableInDevBuild;

	private bool m_IsPurchased = true;

	private bool m_IsLoading;

	private bool m_IsMounted = true;

	private static Dictionary<string, bool> s_OverrideEnable = new Dictionary<string, bool>();

	private static Dictionary<string, DLCStatus> s_OverrideAvailable = new Dictionary<string, DLCStatus>();

	public bool IsAvailableInEditor => m_IsAvailableInEditor;

	public bool IsAvailableInDevBuild => m_IsAvailableInDevBuild;

	public override bool IsSuitable => BuildModeUtility.CheatStoreEnabled;

	public override bool AllowsInstalling => true;

	public override bool AllowsDeleting => true;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on StoreCheat.";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		if (0 == 0)
		{
			value = DLCStatus.UnAvailable;
			return true;
		}
		value = new DLCStatus
		{
			Purchased = m_IsPurchased,
			DownloadState = (m_IsMounted ? DownloadState.Loaded : (m_IsLoading ? DownloadState.Loading : DownloadState.NotLoaded)),
			IsMounted = m_IsMounted
		};
		return true;
	}

	public override bool OpenShop()
	{
		bool isAvailable = false;
		if (!IsSuitable)
		{
			return false;
		}
		try
		{
			UIUtility.ShowMessageBox("Would you like to buy DLC: " + ((base.OwnerBlueprint as BlueprintDlc)?.DlcDisplayName ?? base.OwnerBlueprint.name), DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					isAvailable = true;
					m_IsPurchased = true;
					StoreManager.RefreshAllDLCStatuses();
					StartDownloadAsync();
				}
			});
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return isAvailable;
	}

	public override bool Mount()
	{
		return false;
	}

	public override bool Delete()
	{
		StartDeleteAsync();
		return true;
	}

	private async Task StartDeleteAsync()
	{
		m_IsMounted = false;
		await Task.Delay(3000);
		StoreManager.RefreshAllDLCStatuses();
	}

	public override bool Install()
	{
		StartDownloadAsync();
		return true;
	}

	private async Task StartDownloadAsync()
	{
		await Task.Delay(1000);
		m_IsLoading = true;
		StoreManager.RefreshAllDLCStatuses();
		await Task.Delay(3000);
		m_IsLoading = false;
		m_IsMounted = true;
		StoreManager.RefreshAllDLCStatuses();
	}

	public static void AvailableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideAvailable.TryAdd(dlc.AssetGuid, DLCStatus.Available))
		{
			DLCStatus dLCStatus = s_OverrideAvailable[dlc.AssetGuid];
			dLCStatus.Purchased = true;
			dLCStatus.DownloadState = DownloadState.Loaded;
			dLCStatus.IsMounted = true;
		}
	}

	public static void UnAvailableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideAvailable.TryAdd(dlc.AssetGuid, DLCStatus.UnAvailable))
		{
			DLCStatus dLCStatus = s_OverrideAvailable[dlc.AssetGuid];
			dLCStatus.Purchased = false;
			dLCStatus.DownloadState = DownloadState.NotLoaded;
			dLCStatus.IsMounted = false;
		}
	}

	public static void EnableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideEnable.TryAdd(dlc.AssetGuid, value: true))
		{
			s_OverrideEnable[dlc.AssetGuid] = true;
		}
	}

	public static void DisableDlc(BlueprintDlc dlc)
	{
		if (dlc != null && !string.IsNullOrEmpty(dlc.AssetGuid) && !s_OverrideEnable.TryAdd(dlc.AssetGuid, value: false))
		{
			s_OverrideEnable[dlc.AssetGuid] = false;
		}
	}

	public static bool TryIsDlcEnable([CanBeNull] BlueprintDlc dlc, out bool result)
	{
		result = false;
		if (dlc == null || string.IsNullOrEmpty(dlc.AssetGuid) || !s_OverrideEnable.ContainsKey(dlc.AssetGuid))
		{
			return false;
		}
		result = s_OverrideEnable[dlc.AssetGuid];
		return true;
	}
}
