using System;
using System.Collections;
using ExitGames.Client.Photon;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Settings;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.Reporting.Base;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Device;

namespace Kingmaker.Networking;

public class BugReportNetManager
{
	private class BugReportNetManagerContext : ContextFlag<BugReportNetManagerContext>
	{
	}

	public void Sync(string id, string text, string type)
	{
		if (!NetworkingManager.IsMultiplayer)
		{
			PFLog.Net.Log("[BugReportNetManager] Singleplayer is active -- ignoring...");
			return;
		}
		if ((bool)ContextData<BugReportNetManagerContext>.Current)
		{
			PFLog.Net.Log("[BugReportNetManager] Already synced -- skipping...");
			return;
		}
		try
		{
			ByteArraySlice bytes = NetMessageSerializer.SerializeToSlice(new RequestBugReportMessage(id, text, type));
			if (!PhotonManager.Instance.SendMessageToOthers(12, bytes))
			{
				PFLog.Net.Error("[BugReportNetManager] Failed to send the message!");
			}
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex, "Can't send RequestBugReportMessage!");
		}
	}

	public void OnMessage(ReadOnlySpan<byte> bytes)
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			PFLog.Net.Log("[BugReportNetManager] IsDevelopment is false. Ignoring message...");
			return;
		}
		if (!SettingsRoot.Game.Main.SendGameStatistic)
		{
			PFLog.Net.Log("[BugReportNetManager] SendGameStatistic is false. Ignoring message...");
			return;
		}
		if (!ReportingCheats.IsNetReport)
		{
			PFLog.Net.Warning("[BugReportNetManager] IsNetReport is false. Ignoring message...");
			return;
		}
		RequestBugReportMessage message;
		try
		{
			message = NetMessageSerializer.DeserializeFromSpan<RequestBugReportMessage>(bytes);
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex, "Can't parse RequestBugReportMessage!");
			return;
		}
		PFLog.Net.Log($"Duplicating multiplayer bug report id={message.Id} player={NetworkingManager.LocalNetPlayer.Index}/{NetworkingManager.PlayersCount}");
		MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(SendReportCoroutine(message));
	}

	private static IEnumerator SendReportCoroutine(RequestBugReportMessage message)
	{
		yield return ReportingUtils.Instance.MakeNewReport(makeScreenshot: true, makeSave: true, addCrashDump: false);
		using (ContextData<BugReportNetManagerContext>.Request())
		{
			ReportingUtils.Instance.SendReport(message.Text, string.Empty, SystemInfo.deviceUniqueIdentifier, message.Type, "", isSendMarketing: false, message.Id);
		}
	}
}
