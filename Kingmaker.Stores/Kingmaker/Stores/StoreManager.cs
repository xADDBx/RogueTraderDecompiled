using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.EOSSDK;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Plugins.GOG;
using UnityEngine;

namespace Kingmaker.Stores;

public static class StoreManager
{
	private enum EgsDlcStatus
	{
		None,
		Received,
		ReceiveFailed
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("StoreManager");

	private static bool s_Detected;

	public static readonly DLCCache DLCCache = new DLCCache();

	private static StoreType s_Store;

	private static bool? s_EgsSignInResult;

	private static EgsDlcStatus s_EgsDlcStatuses = EgsDlcStatus.None;

	private static IEnumerable<IBlueprintDlc> s_Dlcs => InterfaceServiceLocator.TryGetService<IDlcRootService>()?.Dlcs ?? Enumerable.Empty<IBlueprintDlc>();

	public static StoreType Store
	{
		get
		{
			if (!s_Detected)
			{
				DetectStore();
			}
			return s_Store;
		}
	}

	public static bool IsEgsDlcStatusAwaiting
	{
		get
		{
			if (Store == StoreType.EpicGames)
			{
				if (s_EgsSignInResult.HasValue)
				{
					if (s_EgsSignInResult.Value)
					{
						return s_EgsDlcStatuses == EgsDlcStatus.None;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public static event Action OnRefreshDLC;

	public static event Action OnSignIn;

	private static void DetectStore()
	{
		s_Detected = true;
		s_Store = StoreType.None;
		if (File.Exists(Path.Combine(Application.dataPath, "steam.info")))
		{
			s_Store = StoreType.Steam;
		}
		if (File.Exists(Path.Combine(Application.dataPath, "gog.info")))
		{
			s_Store = StoreType.GoG;
		}
		if (File.Exists(Path.Combine(Application.dataPath, "discord.info")))
		{
			s_Store = StoreType.Discord;
		}
		if (File.Exists(Path.Combine(Application.dataPath, "egs.info")))
		{
			s_Store = StoreType.EpicGames;
		}
		Logger.Log($"Detected store type: {s_Store}");
	}

	public static void RaiseOnSignIn()
	{
		StoreManager.OnSignIn?.Invoke();
		RefreshAllDLCStatuses();
	}

	private static void RaiseOnSignInEGS(bool result)
	{
		s_EgsSignInResult = result;
		Logger.Log($"Handle EGS sign in {result}");
		if (result)
		{
			RefreshAllDLCStatusesAsync();
		}
	}

	private static void RaiseOnInitializeGOG()
	{
		RefreshAllDLCStatuses();
	}

	private static async void RefreshAllDLCStatusesAsync()
	{
		try
		{
			if (Store == StoreType.EpicGames)
			{
				foreach (IBlueprintDlc s_Dlc in s_Dlcs)
				{
					IDLCStoreEpic iDLCStoreEpic = s_Dlc.GetDlcStores().OfType<IDLCStoreEpic>().SingleOrDefault();
					if (iDLCStoreEpic != null && !string.IsNullOrEmpty(iDLCStoreEpic.EpicId))
					{
						EpicGamesManager.DlcHelper.AddIDToQueryOwnershipList(iDLCStoreEpic.EpicId);
					}
				}
				await EpicGamesManager.DlcHelper.ExecuteQueryOwnership();
			}
		}
		catch (Exception ex)
		{
			s_EgsDlcStatuses = EgsDlcStatus.ReceiveFailed;
			Logger.Exception(ex, "Refresh EGS dlc statuses failed");
			throw;
		}
		RefreshAllDLCStatuses();
	}

	public static void InitializeStore()
	{
		switch (Store)
		{
		case StoreType.Steam:
			Logger.Log("Initializing store: Steam");
			_ = SteamManager.Instance;
			if (SteamManager.Initialized)
			{
				RefreshAllDLCStatuses();
			}
			break;
		case StoreType.GoG:
		{
			Logger.Log("Initializing store: GOG");
			if (GogGalaxyManager.IsInitialized())
			{
				RefreshAllDLCStatuses();
			}
			else
			{
				GogGalaxyManager.OnInitialize += RaiseOnInitializeGOG;
				GogGalaxyManager.StartManager();
			}
			string text = string.Join(", ", Directory.GetFiles(Path.Combine(Application.dataPath, ".."), "goggame*.info"));
			Logger.Log("Detected infos: " + text);
			break;
		}
		case StoreType.Discord:
			DiscordRunner.Initialize();
			break;
		case StoreType.EpicGames:
			Logger.Log("Initializing store: Epic Games");
			if (EpicGamesManager.IsInitializedAndSignedIn())
			{
				RaiseOnSignInEGS(result: true);
				break;
			}
			EpicGamesManager.OnSignIn += RaiseOnSignInEGS;
			EpicGamesManager.StartManager(Application.isEditor);
			break;
		default:
			Logger.Log($"Initializing store: unknown store type {Store}");
			break;
		case StoreType.None:
			break;
		}
	}

	public static void OpenShopFor(IBlueprintDlc dlc)
	{
		bool flag = false;
		foreach (IDlcStore dlcStore in dlc.GetDlcStores())
		{
			if (dlcStore.IsSuitable)
			{
				flag = true;
				dlcStore.OpenShop();
				break;
			}
		}
		if (!flag)
		{
			Logger.Error($"Not found shop for {dlc}");
		}
	}

	public static void DeleteDlc(IBlueprintDlc dlc)
	{
		Logger.Log($"deleting {dlc}");
		IEnumerable<IDlcStore> dlcStores = dlc.GetDlcStores();
		foreach (IDlcStore item in dlcStores)
		{
			if (item.IsSuitable && item.AllowsDeleting)
			{
				if (item.Delete())
				{
					UpdateDLCCache(dlc, dlcStores);
				}
				break;
			}
		}
	}

	public static void InstallDlc(IBlueprintDlc dlc)
	{
		Logger.Log($"installing {dlc}");
		IEnumerable<IDlcStore> dlcStores = dlc.GetDlcStores();
		foreach (IDlcStore item in dlcStores)
		{
			if (item.IsSuitable && item.AllowsInstalling)
			{
				if (item.Install())
				{
					UpdateDLCCache(dlc, dlcStores);
				}
				break;
			}
		}
	}

	public static void MountDlc(IBlueprintDlc dlc)
	{
		Logger.Log($"mounting {dlc}");
		IEnumerable<IDlcStore> dlcStores = dlc.GetDlcStores();
		foreach (IDlcStore item in dlcStores)
		{
			if (item.IsSuitable && item.AllowsPurchase)
			{
				if (item.Mount())
				{
					UpdateDLCCache(dlc, dlcStores);
				}
				break;
			}
		}
	}

	public static void UpdateDLCStatus(IBlueprintDlc dlc, DLCStatus newStatus)
	{
		if (newStatus == null)
		{
			return;
		}
		IDLCStatus other = DLCCache.Get(dlc);
		if (newStatus.Equals(other))
		{
			return;
		}
		DLCCache.OnDLCUpdate(dlc, newStatus);
		if (!newStatus.Purchased || !newStatus.IsMounted)
		{
			return;
		}
		try
		{
			foreach (IBlueprintDlcReward reward in dlc.Rewards)
			{
				reward.RecheckAvailability();
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	public static IEnumerable<IBlueprintDlc> GetPurchasableDLCs()
	{
		foreach (IBlueprintDlc s_Dlc in s_Dlcs)
		{
			if (s_Dlc.GetDlcStores().TryFind((IDlcStore x) => x.IsSuitable && x.AllowsPurchase, out var _))
			{
				yield return s_Dlc;
			}
		}
	}

	public static IEnumerable<IBlueprintDlc> GetAllAvailableAdditionalContentDlc()
	{
		foreach (IBlueprintDlc s_Dlc in s_Dlcs)
		{
			if (s_Dlc.IsAvailable && s_Dlc.DlcType == DlcTypeEnum.AdditionalContentDlc)
			{
				yield return s_Dlc;
			}
		}
	}

	public static void RefreshAllDLCStatuses()
	{
		s_EgsDlcStatuses = EgsDlcStatus.Received;
		RefreshDLCs(s_Dlcs);
	}

	private static void RefreshDLCs(IEnumerable<IBlueprintDlc> dlcs)
	{
		Logger.Log("Refreshing dlc list");
		foreach (IBlueprintDlc dlc in dlcs)
		{
			try
			{
				UpdateDLCCache(dlc);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex, $"Exception checking DLC {dlc}");
			}
		}
		StoreManager.OnRefreshDLC?.Invoke();
	}

	private static void UpdateDLCCache(IBlueprintDlc dlc, [CanBeNull] IEnumerable<IDlcStore> dlcStores = null)
	{
		bool flag = false;
		if (dlcStores == null)
		{
			dlcStores = dlc.GetDlcStores();
		}
		foreach (IDlcStore dlcStore in dlcStores)
		{
			if (dlcStore.IsSuitable && dlcStore.TryGetStatus(out var value))
			{
				Logger.Log($"[{dlcStore}] DlcStatus for {dlc}: purchased = {value.Purchased}, download = {value.DownloadState}, mounted = {value.IsMounted}");
				DLCCache.OnDLCUpdate(dlc, value);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			DLCCache.OnDLCUpdate(dlc, DLCStatus.UnAvailable);
		}
	}
}
