using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Code.GameCore.Mics;
using Kingmaker.EOSSDK;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Plugins.GOG;
using UnityEngine;

namespace Kingmaker.Stores;

public static class StoreManager
{
	private static bool s_Detected;

	public static readonly DLCCache DLCCache = new DLCCache();

	private static StoreType s_Store;

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
		PFLog.System.Log($"Detected store type: {s_Store}");
	}

	public static void RaiseOnSignIn()
	{
		StoreManager.OnSignIn?.Invoke();
		RefreshAllDLCStatuses();
	}

	private static void RaiseOnSignInEGS(bool result)
	{
		PFLog.System.Log($"Handle EGS sign in {result}");
		if (result)
		{
			UpdateDLCStatus();
		}
	}

	private static async void UpdateDLCStatus()
	{
		await UpdateDLCStatus(s_Dlcs);
	}

	public static void InitializeStore()
	{
		switch (Store)
		{
		case StoreType.Steam:
			PFLog.System.Log("Initializing store: Steam");
			_ = SteamManager.Instance;
			if (SteamManager.Initialized)
			{
				RefreshAllDLCStatuses();
			}
			break;
		case StoreType.GoG:
		{
			PFLog.System.Log("Initializing store: GOG");
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
			PFLog.System.Log("Detected infos: " + text);
			break;
		}
		case StoreType.Discord:
			DiscordRunner.Initialize();
			break;
		case StoreType.EpicGames:
			PFLog.System.Log("Initializing store: Epic Games");
			if (EpicGamesManager.IsInitializedAndSignedIn())
			{
				RaiseOnSignInEGS(result: true);
				break;
			}
			EpicGamesManager.OnSignIn += RaiseOnSignInEGS;
			EpicGamesManager.StartManager(Application.isEditor);
			break;
		default:
			PFLog.System.Log($"Initializing store: unknown store type {Store}");
			break;
		case StoreType.None:
			break;
		}
	}

	private static void RaiseOnInitializeGOG()
	{
		RefreshAllDLCStatuses();
	}

	public static async Task UpdateDLCStatus(IEnumerable<IBlueprintDlc> dlcs)
	{
		PFLog.System.Log("[StoreManager] UpdateDLCStatus()");
		switch (Store)
		{
		case StoreType.EpicGames:
			foreach (IBlueprintDlc dlc in dlcs)
			{
				IDLCStoreEpic iDLCStoreEpic = dlc.GetDlcStores().OfType<IDLCStoreEpic>().SingleOrDefault();
				if (iDLCStoreEpic != null && !string.IsNullOrEmpty(iDLCStoreEpic.EpicId))
				{
					EpicGamesManager.DlcHelper.AddIDToQueryOwnershipList(iDLCStoreEpic.EpicId);
				}
			}
			await EpicGamesManager.DlcHelper.ExecuteQueryOwnership();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case StoreType.None:
		case StoreType.Steam:
		case StoreType.GoG:
		case StoreType.Discord:
			break;
		}
		RefreshDLCs(dlcs);
	}

	public static void OpenShopFor(IBlueprintDlc dlc)
	{
		bool flag = false;
		foreach (IDlcStore dlcStore in dlc.GetDlcStores())
		{
			if (dlcStore.IsSuitable && dlcStore.AllowsPurchase)
			{
				flag = true;
				dlcStore.OpenShop();
				break;
			}
		}
		if (!flag)
		{
			PFLog.Default.Error($"[StoreManager] Not found shop for {dlc}");
		}
	}

	public static void MountDlc(IBlueprintDlc dlc)
	{
		PFLog.Default.Log($"[StoreManager] mounting {dlc}");
		IEnumerable<IDlcStore> dlcStores = dlc.GetDlcStores();
		foreach (IDlcStore item in dlcStores)
		{
			if (item.IsSuitable && item.AllowsPurchase)
			{
				if (item.Mount())
				{
					UpdateDLCStatus(dlc, dlcStores);
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
			PFLog.Default.Exception(ex);
		}
	}

	public static IEnumerable<IBlueprintDlc> GetPurchasableDLCs()
	{
		foreach (IBlueprintDlc s_Dlc in s_Dlcs)
		{
			if (s_Dlc.GetDlcStores().TryFind((IDlcStore x) => x.AllowsPurchase, out var _))
			{
				yield return s_Dlc;
			}
		}
	}

	public static void RefreshAllDLCStatuses()
	{
		RefreshDLCs(s_Dlcs);
	}

	private static void RefreshDLCs(IEnumerable<IBlueprintDlc> dlcs)
	{
		PFLog.Default.Log("Refreshing dlc list");
		foreach (IBlueprintDlc dlc in dlcs)
		{
			try
			{
				UpdateDLCStatus(dlc);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, $"Exception checking DLC {dlc}");
			}
		}
		StoreManager.OnRefreshDLC?.Invoke();
	}

	public static void UpdateDLCStatus(IBlueprintDlc dlc)
	{
		IEnumerable<IDlcStore> dlcStores = dlc.GetDlcStores();
		UpdateDLCStatus(dlc, dlcStores);
	}

	private static void UpdateDLCStatus(IBlueprintDlc dlc, IEnumerable<IDlcStore> dlcStores)
	{
		bool flag = false;
		foreach (IDlcStore dlcStore in dlcStores)
		{
			if (dlcStore.IsSuitable)
			{
				IDLCStatus status = dlcStore.GetStatus();
				if (status != null)
				{
					PFLog.Default.Log($"[StoreManager] DlcStatus for {dlc}: purchased = {status.Purchased}, download = {status.DownloadState}, mounted = {status.IsMounted}, enabled={status.IsEnabled}");
					DLCCache.OnDLCUpdate(dlc, status);
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			DLCCache.OnDLCUpdate(dlc, new DLCStatus());
		}
	}
}
