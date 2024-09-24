using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Presence;
using JetBrains.Annotations;
using Kingmaker.EOSSDK;
using Photon.Realtime;

namespace Kingmaker.Networking.Platforms;

public sealed class EpicGamesStorePlatformInvite : IPlatformInvite, IDisposable
{
	private readonly EpicGamesInviteHelper m_InviteHelper;

	private ulong m_JoinGameAcceptedHandle;

	private static string Region => PhotonManager.Instance.Region;

	private static string RoomName => PhotonManager.Instance.RoomName;

	private static List<Region> EnabledRegions => null;

	public EpicGamesStorePlatformInvite()
	{
		m_InviteHelper = EpicGamesManager.InviteHelper;
		m_InviteHelper.ReceiveInvitePayload += OnReceiveInvitePayload;
		if (m_InviteHelper.TryGetInvitePayload(out var payload))
		{
			OnReceiveInvitePayload(payload);
		}
		InitJoinCallback();
	}

	private async void InitJoinCallback()
	{
		if ((object)EpicGamesManager.Instance == null)
		{
			throw new NullReferenceException("EpicGamesManager.Instance is null");
		}
		if (!(await EpicGamesManager.Instance.WaitForSignIn()))
		{
			PFLog.Net.Error("[EpicGamesStorePlatformInvite.InitJoinCallback] signin failed");
			return;
		}
		if ((object)EpicGamesManager.EOSPresenceInterface == null)
		{
			throw new NullReferenceException("EOSPresenceInterface is null");
		}
		AddNotifyJoinGameAcceptedOptions options = default(AddNotifyJoinGameAcceptedOptions);
		m_JoinGameAcceptedHandle = EpicGamesManager.EOSPresenceInterface.AddNotifyJoinGameAccepted(ref options, null, delegate(ref JoinGameAcceptedCallbackInfo data)
		{
			PFLog.Net.Log($"[EpicGamesStorePlatformInvite] join game accepted with data '{data.JoinInfo}'");
			if (NetRoomNameHelper.TryParse(data.JoinInfo, EnabledRegions, out var server, out var room))
			{
				PhotonManager.Invite.AcceptInvite(server, room);
			}
		});
	}

	private void OnReceiveInvitePayload(string payload)
	{
		m_InviteHelper.FinalizeInvite();
		if (NetRoomNameHelper.TryParse(payload, EnabledRegions, out var server, out var room))
		{
			PhotonManager.Invite.AcceptInvite(server, room);
		}
	}

	public bool TryGetInviteRoom(out string roomServer, out string roomName)
	{
		if (m_InviteHelper.TryGetInvitePayload(out var payload))
		{
			m_InviteHelper.FinalizeInvite();
			return NetRoomNameHelper.TryParse(payload, EnabledRegions, out roomServer, out roomName);
		}
		roomServer = (roomName = null);
		return false;
	}

	public bool IsSupportInviteWindow()
	{
		return true;
	}

	public void ShowInviteWindow()
	{
		if (NetRoomNameHelper.TryFormatString(Region, RoomName, out var output))
		{
			m_InviteHelper.ShowInviteOverlay(output);
		}
	}

	public void Invite(string userId)
	{
		if (NetRoomNameHelper.TryFormatString(Region, RoomName, out var output))
		{
			m_InviteHelper.Invite(output, userId);
		}
	}

	public void StartAnnounceGame()
	{
		if (NetRoomNameHelper.TryFormatString(Region, RoomName, out var output))
		{
			SetJoinInfo(output);
			m_InviteHelper.SetInviteData(output);
		}
	}

	public void StopAnnounceGame()
	{
		SetJoinInfo(null);
		m_InviteHelper.SetInviteData(null);
	}

	public void Dispose()
	{
		m_InviteHelper.ReceiveInvitePayload -= OnReceiveInvitePayload;
		EpicGamesManager.EOSPresenceInterface.RemoveNotifyJoinGameAccepted(m_JoinGameAcceptedHandle);
	}

	private void SetJoinInfo([CanBeNull] string joinInfo)
	{
		CreatePresenceModificationOptions createPresenceModificationOptions = default(CreatePresenceModificationOptions);
		createPresenceModificationOptions.LocalUserId = EpicGamesManager.LocalUser.AccountId;
		CreatePresenceModificationOptions options = createPresenceModificationOptions;
		PresenceModification outPresenceModificationHandle;
		Result result = EpicGamesManager.EOSPresenceInterface.CreatePresenceModification(ref options, out outPresenceModificationHandle);
		if (result != 0)
		{
			PFLog.Net.Error($"[EpicGamesStorePlatformInvite] CreatePresenceModification failed with result {result}");
			return;
		}
		PresenceModificationSetJoinInfoOptions presenceModificationSetJoinInfoOptions = default(PresenceModificationSetJoinInfoOptions);
		presenceModificationSetJoinInfoOptions.JoinInfo = joinInfo;
		PresenceModificationSetJoinInfoOptions options2 = presenceModificationSetJoinInfoOptions;
		outPresenceModificationHandle.SetJoinInfo(ref options2);
		SetPresenceOptions setPresenceOptions = default(SetPresenceOptions);
		setPresenceOptions.LocalUserId = EpicGamesManager.LocalUser.AccountId;
		setPresenceOptions.PresenceModificationHandle = outPresenceModificationHandle;
		SetPresenceOptions options3 = setPresenceOptions;
		EpicGamesManager.EOSPresenceInterface.SetPresence(ref options3, null, delegate(ref SetPresenceCallbackInfo data)
		{
			PFLog.Net.Log($"[EpicGamesStorePlatformInvite.SetJoinInfo] {data.ResultCode}");
		});
		outPresenceModificationHandle.Release();
	}
}
