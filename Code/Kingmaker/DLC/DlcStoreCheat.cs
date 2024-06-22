using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
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

	private static Dictionary<string, bool> s_OverrideEnable = new Dictionary<string, bool>();

	public bool IsAvailableInEditor => m_IsAvailableInEditor;

	public bool IsAvailableInDevBuild => m_IsAvailableInDevBuild;

	public override bool IsSuitable => GetStatus() != null;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on StoreCheat.";

	public override IDLCStatus GetStatus()
	{
		_ = (base.OwnerBlueprint as IBlueprintDlc)?.DlcType ?? DlcTypeEnum.CosmeticDlc;
		if (0 == 0)
		{
			return null;
		}
		return DLCStatus.Available;
	}

	public override bool OpenShop()
	{
		bool result = false;
		if (!IsSuitable)
		{
			return false;
		}
		try
		{
			Application.OpenURL(m_TestShopLink);
			result = true;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return result;
	}

	public override bool Mount()
	{
		return false;
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
