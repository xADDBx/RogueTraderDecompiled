using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyPlayerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly BoolReactiveProperty IsMe = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsMeHost = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsEmpty = new BoolReactiveProperty(initialValue: true);

	public readonly BoolReactiveProperty IsPlaying = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty(initialValue: true);

	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>();

	private PhotonActorNumber m_UserNumber;

	private readonly ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> m_DifferentPlatformInviteVM;

	public readonly BoolReactiveProperty EpicGamesAuthorized;

	public readonly StringReactiveProperty PlayerDLcList = new StringReactiveProperty();

	public NetLobbyPlayerVM(ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> differentPlatformInviteVM, BoolReactiveProperty epicGamesAuthorized)
	{
		m_DifferentPlatformInviteVM = differentPlatformInviteVM;
		EpicGamesAuthorized = epicGamesAuthorized;
		ClearPlayer();
	}

	protected NetLobbyPlayerVM()
	{
		ClearPlayer();
	}

	protected override void DisposeImplementation()
	{
	}

	public void ClearPlayer()
	{
		IsEmpty.Value = true;
		PlayerDLcList.Value = string.Empty;
		IsMe.Value = false;
		IsMeHost.Value = PhotonManager.Instance != null && PhotonManager.Instance.IsRoomOwner;
		IsPlaying.Value = PhotonManager.Instance != null && PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		IsActive.Value = true;
		m_UserNumber = PhotonActorNumber.Invalid;
		Name.Value = string.Empty;
		Portrait.Value = null;
	}

	public virtual void SetPlayer(PhotonActorNumber player, string userId, bool isActive)
	{
		IsEmpty.Value = false;
		IsMe.Value = userId.Equals(PhotonManager.Instance.LocalPlayerUserId, StringComparison.Ordinal);
		IsMeHost.Value = PhotonManager.Instance.IsRoomOwner;
		IsPlaying.Value = PhotonManager.NetGame.CurrentState == NetGame.State.Playing;
		MainThreadDispatcher.StartCoroutine(WaitForDlcsUpdated(delegate
		{
			CheckPlayerDlcList(userId);
		}));
		m_UserNumber = player;
		Name.Value = ((PhotonManager.Player.GetNickName(player, out var nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : userId);
		Portrait.Value = player.GetPlayerIcon();
		IsActive.Value = isActive;
	}

	public IEnumerator WaitForDlcsUpdated(Action finishAction)
	{
		while (!PhotonManager.DLC.IsDLCsInLobbyReady)
		{
			PFLog.UI.Log("[WaitForDlcsUpdated] waiting for dlcs...");
			yield return null;
		}
		finishAction();
	}

	private void CheckPlayerDlcList(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			return;
		}
		List<IBlueprintDlc> playerDLCs = new List<IBlueprintDlc>();
		PhotonManager.DLC.TryGetPlayerDLC(userId, out playerDLCs);
		if (playerDLCs == null || !playerDLCs.Any())
		{
			PlayerDLcList.Value = string.Empty;
			return;
		}
		List<string> list = playerDLCs.Select(delegate(IBlueprintDlc playerDLC)
		{
			BlueprintDlc blueprintDlc = playerDLC as BlueprintDlc;
			return string.IsNullOrWhiteSpace(blueprintDlc?.DefaultTitle) ? blueprintDlc?.name : ((string)blueprintDlc?.DefaultTitle);
		}).ToList();
		PlayerDLcList.Value = (list.Any() ? string.Join(Environment.NewLine, list) : string.Empty);
	}

	public void Kick()
	{
		UIUtility.ShowMessageBox(string.Format(UIStrings.Instance.NetLobbyTexts.KickPlayerMessage, Name.Value), DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				PhotonManager.Instance.KickPlayer(m_UserNumber);
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
		PlatformServices.Platform.Invite.ShowInviteWindow();
		HideNetLobbyDifferentPlatformsInvite();
	}

	public async void InviteFromSecondaryStore()
	{
		HideNetLobbyDifferentPlatformsInvite();
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
			return;
		}
		m_DifferentPlatformInviteVM.Value = new NetLobbyInvitePlayerDifferentPlatformsVM(delegate
		{
			HideNetLobbyDifferentPlatformsInvite();
		}, delegate
		{
			InviteFromPrimaryStore();
		}, delegate
		{
			InviteFromSecondaryStore();
		});
	}

	private void HideNetLobbyDifferentPlatformsInvite()
	{
		m_DifferentPlatformInviteVM.Value?.Dispose();
		m_DifferentPlatformInviteVM.Value = null;
	}
}
