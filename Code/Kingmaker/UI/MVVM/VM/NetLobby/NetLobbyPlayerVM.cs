using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyPlayerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetDLCsHandler, ISubscriber, INetLobbyPlayersHandler
{
	public readonly BoolReactiveProperty IsMe = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsMeHost = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsEmpty = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsPlaying = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty(initialValue: true);

	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> UserId = new ReactiveProperty<string>();

	private readonly ReactiveProperty<PhotonActorNumber> m_UserNumber = new ReactiveProperty<PhotonActorNumber>();

	private readonly ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> m_DifferentPlatformInviteVM;

	public readonly BoolReactiveProperty EpicGamesAuthorized;

	public readonly StringReactiveProperty PlayerDLcStringList = new StringReactiveProperty();

	public readonly GamerTagAndNameVM GamerTagAndNameVM;

	public readonly StringReactiveProperty PlayersDifferentDlcs = new StringReactiveProperty();

	public NetLobbyPlayerVM(ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> differentPlatformInviteVM, BoolReactiveProperty epicGamesAuthorized)
	{
		m_DifferentPlatformInviteVM = differentPlatformInviteVM;
		EpicGamesAuthorized = epicGamesAuthorized;
		AddDisposable(GamerTagAndNameVM = new GamerTagAndNameVM(UserId, m_UserNumber, Name));
		ClearPlayer();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected NetLobbyPlayerVM()
	{
		AddDisposable(GamerTagAndNameVM = new GamerTagAndNameVM(UserId, m_UserNumber, Name));
		ClearPlayer();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void ClearPlayer()
	{
		IsEmpty.Value = true;
		PlayerDLcStringList.SetValueAndForceNotify(string.Empty);
		IsMe.Value = false;
		IsMeHost.Value = PhotonManager.Instance != null && PhotonManager.Instance.IsRoomOwner;
		IsPlaying.Value = PhotonManager.Instance != null && PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		IsActive.Value = true;
		UserId.Value = null;
		m_UserNumber.Value = PhotonActorNumber.Invalid;
		Name.Value = string.Empty;
		Portrait.Value = null;
		PlayerDLcStringList.SetValueAndForceNotify(null);
	}

	public virtual void SetPlayer(PhotonActorNumber player, string userId, bool isActive)
	{
		IsEmpty.Value = false;
		IsMe.Value = userId.Equals(PhotonManager.Instance.LocalPlayerUserId, StringComparison.Ordinal);
		IsMeHost.Value = PhotonManager.Instance.IsRoomOwner;
		IsPlaying.Value = PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		UserId.Value = userId;
		m_UserNumber.Value = player;
		Name.Value = ((PhotonManager.Player.GetNickName(player, out var nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : string.Empty);
		PFLog.Net.Log((!string.IsNullOrWhiteSpace(nickName)) ? ("NetLobbyPlayerVM SET NICKNAME " + nickName) : "NetLobbyPlayerVM SET USER ID EMPTY STRING");
		Portrait.Value = player.GetPlayerIcon();
		IsActive.Value = isActive;
	}

	private string GetDLCsStringList(string userID)
	{
		if (string.IsNullOrEmpty(userID))
		{
			return null;
		}
		List<IBlueprintDlc> playerDlcs = GetPlayerDlcs(userID);
		if (playerDlcs == null || !playerDlcs.Any())
		{
			return null;
		}
		if (0 >= playerDlcs.Count)
		{
			return UIStrings.Instance.NetLobbyTexts.PlayerHasNoDlcs;
		}
		IEnumerable<string> values = playerDlcs.OrderBy((IBlueprintDlc dlc) => dlc.DlcType).ToList().Select(delegate(IBlueprintDlc playerDLC)
		{
			if (!(playerDLC is BlueprintDlc blueprintDlc))
			{
				return (string)null;
			}
			return string.IsNullOrEmpty(blueprintDlc.GetDlcName()) ? blueprintDlc.name : blueprintDlc.GetDlcName();
		});
		return string.Join(Environment.NewLine, values);
	}

	private string CompareHostAndPlayerDlcs()
	{
		PlayerInfo playerInfo = PhotonManager.Instance.AllPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (playerInfo.UserId == UserId.Value)
		{
			return null;
		}
		if (string.IsNullOrWhiteSpace(UserId.Value) || playerInfo.UserId == null)
		{
			return null;
		}
		List<BlueprintDlc> second = GetPlayerDlcs(UserId.Value).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType2 = dlc.DlcType;
			return dlcType2 != DlcTypeEnum.CosmeticDlc && dlcType2 != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		List<BlueprintDlc> list = GetPlayerDlcs(playerInfo.UserId).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType = dlc.DlcType;
			return dlcType != DlcTypeEnum.CosmeticDlc && dlcType != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		if (!list.Any())
		{
			return null;
		}
		List<BlueprintDlc> source = list.Except(second).ToList();
		if (!source.Any())
		{
			return null;
		}
		IEnumerable<string> values = source.OrderBy((BlueprintDlc dlc) => dlc.DlcType).ToList().Select(delegate(BlueprintDlc missingDLC)
		{
			if (missingDLC == null)
			{
				return (string)null;
			}
			return string.IsNullOrEmpty(missingDLC.GetDlcName()) ? missingDLC.name : missingDLC.GetDlcName();
		});
		return string.Join(Environment.NewLine, values);
	}

	private List<IBlueprintDlc> GetPlayerDlcs(string userID)
	{
		if (PhotonManager.DLC.TryGetPlayerDLC(userID, out var playerDLCs))
		{
			return playerDLCs;
		}
		PFLog.UI.Log("[NetLobbyPlayerVM.RefreshDLCsList] DLCs for user='" + userID + "' not found!");
		return new List<IBlueprintDlc>();
	}

	public void Kick()
	{
		UIUtility.ShowMessageBox(string.Format(UIStrings.Instance.NetLobbyTexts.KickPlayerMessage, Name.Value), DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				PhotonManager.Instance.KickPlayer(m_UserNumber.Value);
			}
		});
	}

	public void Invite()
	{
		if (PlatformServices.Platform.HasSecondaryPlatform)
		{
			ShowNetLobbyDifferentPlatformsInvite();
		}
		else
		{
			InviteFromPrimaryStore();
		}
	}

	public void InviteFromPrimaryStore()
	{
		HideNetLobbyDifferentPlatformsInvite();
		if (!PlatformServices.Platform.Invite.IsSupportInviteWindow())
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.StoreOverlayIsNotAvailable, DialogMessageBoxBase.BoxType.Message, null);
		}
		else
		{
			PlatformServices.Platform.Invite.ShowInviteWindow();
		}
	}

	public async void InviteFromSecondaryStore()
	{
		HideNetLobbyDifferentPlatformsInvite();
		if (!PlatformServices.Platform.Invite.IsSupportInviteWindow())
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.StoreOverlayIsNotAvailable, DialogMessageBoxBase.BoxType.Message, null);
			return;
		}
		Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
		if (!secondaryPlatform.IsInitialized())
		{
			await PlatformServices.Platform.InitSecondary();
		}
		secondaryPlatform.Invite.ShowInviteWindow();
	}

	private void ShowNetLobbyDifferentPlatformsInvite()
	{
		if (m_DifferentPlatformInviteVM == null)
		{
			InviteFromPrimaryStore();
		}
		else
		{
			m_DifferentPlatformInviteVM.Value = new NetLobbyInvitePlayerDifferentPlatformsVM(HideNetLobbyDifferentPlatformsInvite, InviteFromPrimaryStore, InviteFromSecondaryStore);
		}
	}

	private void HideNetLobbyDifferentPlatformsInvite()
	{
		m_DifferentPlatformInviteVM.Value?.Dispose();
		m_DifferentPlatformInviteVM.Value = null;
	}

	void INetDLCsHandler.HandleDLCsListChanged()
	{
		PlayerDLcStringList.SetValueAndForceNotify(GetDLCsStringList(UserId.Value));
		PlayersDifferentDlcs.SetValueAndForceNotify(CompareHostAndPlayerDlcs());
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
		PlayersDifferentDlcs.SetValueAndForceNotify(CompareHostAndPlayerDlcs());
	}
}
