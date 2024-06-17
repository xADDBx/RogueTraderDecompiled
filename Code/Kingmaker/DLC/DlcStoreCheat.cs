using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("fd601a0246034ca38414c127dbcc65ea")]
public class DlcStoreCheat : DlcStore
{
	private static readonly Dictionary<IBlueprintDlc, DLCStatus> OverrideDlcStates = new Dictionary<IBlueprintDlc, DLCStatus>();

	[SerializeField]
	[Tooltip("Is the DLC available in editor playmode")]
	private bool m_IsAvailableInEditor;

	[SerializeField]
	[Tooltip("Is the DLC available in development build")]
	private bool m_IsAvailableInDevBuild;

	public bool IsAvailableInEditor => m_IsAvailableInEditor;

	public bool IsAvailableInDevBuild => m_IsAvailableInDevBuild;

	public override bool IsSuitable => GetStatus() != null;

	public static void EnableDlc(IBlueprintDlc dlc)
	{
		OverrideDlcState(dlc, isEnabled: true);
	}

	public static void DisableDlc(IBlueprintDlc dlc)
	{
		OverrideDlcState(dlc, isEnabled: false);
	}

	private static void OverrideDlcState(IBlueprintDlc dlc, bool isEnabled)
	{
		DLCStatus value = new DLCStatus
		{
			Purchased = true,
			DownloadState = DownloadState.Loaded,
			IsMounted = true,
			IsEnabled = isEnabled
		};
		if (!OverrideDlcStates.ContainsKey(dlc))
		{
			OverrideDlcStates.Add(dlc, value);
		}
		else
		{
			OverrideDlcStates[dlc] = value;
		}
	}

	public override IDLCStatus GetStatus()
	{
		bool flag = false;
		if (!(base.OwnerBlueprint is IBlueprintDlc key) || !OverrideDlcStates.TryGetValue(key, out var value))
		{
			if (!flag)
			{
				return null;
			}
			return DLCStatus.Available;
		}
		return value;
	}

	public override bool Mount()
	{
		return false;
	}
}
