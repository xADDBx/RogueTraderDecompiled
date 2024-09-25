using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class UINetUtility
{
	public static bool InLobbyAndPlaying => PhotonManager.NetGame.CurrentState == NetGame.State.Playing;

	public static bool InLobbyAndPlayingOrLoading
	{
		get
		{
			if (!InLobbyAndPlaying && PhotonManager.NetGame.CurrentState != NetGame.State.UploadSaveAndStartLoading)
			{
				return PhotonManager.NetGame.CurrentState == NetGame.State.DownloadSaveAndLoading;
			}
			return true;
		}
	}

	public static bool IsControlMainCharacter()
	{
		return Game.Instance.Player.MainCharacterEntity.IsMyNetRole();
	}

	public static bool CanEditCareer([CanBeNull] this MechanicEntity entry)
	{
		if (entry == null)
		{
			return false;
		}
		if (RootUIContext.Instance.IsCharInfoLevelProgression)
		{
			return entry.CanBeControlled();
		}
		if (RootUIContext.Instance.IsChargenShown || RootUIContext.Instance.IsShipInventoryShown)
		{
			return IsControlMainCharacter();
		}
		return false;
	}

	public static bool IsControlMainCharacterWithWarning(bool needSignalHowToPing = false)
	{
		if (IsControlMainCharacter())
		{
			return true;
		}
		UINetLobbyTexts netLobbyTexts = UIStrings.Instance.NetLobbyTexts;
		string text = netLobbyTexts.WarningPlayerIsNotControlMainCharacter;
		if (needSignalHowToPing)
		{
			text = text + Environment.NewLine + (Game.Instance.IsControllerMouse ? netLobbyTexts.HowToPingCoopLabelPc : netLobbyTexts.HowToPingCoopLabelConsole);
		}
		UIUtility.SendWarning(text);
		return false;
	}

	public static bool IsAlsoControlMainCharacter([CanBeNull] this MechanicEntity entry)
	{
		if (!InLobbyAndPlaying)
		{
			return true;
		}
		PhotonActorNumber player = entry.GetPlayer();
		if (!player.IsValid)
		{
			return false;
		}
		BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
		NetPlayer player2 = player.ToNetPlayer(NetPlayer.Empty);
		return mainCharacterEntity.IsDirectlyControllable(player2);
	}

	public static bool IsAlsoControlMainCharacterWithWarning([CanBeNull] this MechanicEntity entry)
	{
		if (entry.IsAlsoControlMainCharacter())
		{
			return true;
		}
		if (entry.IsDirectlyControllable())
		{
			UIUtility.SendWarning(UIStrings.Instance.NetLobbyTexts.WarningPlayerIsNotControlMainCharacter);
		}
		return false;
	}

	public static bool IsDirectlyControllable([CanBeNull] this MechanicEntity entry)
	{
		return entry.IsDirectlyControllable(NetworkingManager.LocalNetPlayer);
	}

	public static bool IsDirectlyControllable([CanBeNull] this MechanicEntity entry, NetPlayer player)
	{
		if (entry != null && entry.IsDirectlyControllable)
		{
			return entry.IsNetRoleInternal(player);
		}
		return false;
	}

	public static bool IsMyNetRole([CanBeNull] this MechanicEntity entry)
	{
		return entry.IsDirectlyControllable();
	}

	public static bool CanBeControlled([CanBeNull] this MechanicEntity entry)
	{
		return entry.CanBeControlled(NetworkingManager.LocalNetPlayer);
	}

	public static bool CanBeControlled([CanBeNull] this MechanicEntity entry, NetPlayer player)
	{
		return entry.IsNetRoleInternal(player);
	}

	private static bool IsNetRoleInternal([CanBeNull] this MechanicEntity entity, NetPlayer player)
	{
		if (entity.LocalTest(out var isMine))
		{
			return isMine;
		}
		if (Game.Instance.IsSpaceCombat)
		{
			return entity != null;
		}
		if (InLobbyAndPlayingOrLoading)
		{
			if (entity != null)
			{
				return Game.Instance.CoopData.PlayerRole.Can(entity, player);
			}
			return false;
		}
		return true;
	}

	public static PhotonActorNumber GetPlayer(this Entity entity)
	{
		if (entity == null)
		{
			return PhotonActorNumber.Invalid;
		}
		if (!InLobbyAndPlayingOrLoading)
		{
			return PhotonActorNumber.Invalid;
		}
		foreach (PlayerInfo activePlayer in PhotonManager.Instance.ActivePlayers)
		{
			PhotonActorNumber player = activePlayer.Player;
			NetPlayer player2 = player.ToNetPlayer(NetPlayer.Empty);
			if (Game.Instance.CoopData.PlayerRole.Can(entity, player2))
			{
				return player;
			}
		}
		return PhotonActorNumber.Invalid;
	}

	public static Sprite GetPlayerIcon(this PhotonActorNumber player)
	{
		PhotonManager.Player.GetIconLarge(player, out var value);
		if (!(value != null))
		{
			return BlueprintRoot.Instance.UIConfig.DefaultNetAvatar;
		}
		return Sprite.Create(value, new Rect(0f, 0f, value.width, value.height), new Vector2(0.5f, 0.5f));
	}

	public static string GetUnitNameWithPlayer(this MechanicEntityUIWrapper entity)
	{
		if (!entity.IsPlayer)
		{
			return entity.Name;
		}
		PhotonActorNumber player = entity.MechanicEntity.GetPlayer();
		if (!player.IsValid || entity.MechanicEntity.IsMyNetRole() || !PhotonManager.Player.GetNickName(player, out var nickName))
		{
			return entity.Name;
		}
		return "[" + nickName + "] " + entity.Name;
	}

	public static void ShowBlockedPlayerWarning(string playerName)
	{
		UIUtility.SendWarning(string.Format(UIStrings.Instance.NetLobbyTexts.BlockedPlayerInLobby, playerName));
	}

	public static void ShowCantJoinByPrivacySettingsWarning()
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.CantJoinLobbyDuePrivacySettings, DialogMessageBoxBase.BoxType.Message, null);
	}

	public static void HandlePhotonDisconnectedError(DisconnectCause cause, bool allowReconnect)
	{
		string text = UIStrings.Instance.NetLobbyErrorsTexts.PhotonDisconnectedErrorMessage.Text + " " + ReasonStrings.Instance.GetDisconnectCause(cause);
		if (allowReconnect)
		{
			ShowReconnectDialog(text);
			return;
		}
		UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Message, delegate
		{
			CloseLobby();
		});
	}

	public static void ShowReconnectDialog(string message)
	{
		UIUtility.ShowMessageBox(message, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton btn)
		{
			if (btn == DialogMessageBoxBase.BoxButton.Yes)
			{
				PhotonManager.NetGame.StartNetGameIfNeeded();
			}
			else
			{
				CloseLobby();
			}
		}, null, UIStrings.Instance.NetLobbyTexts.Reconnect, UIStrings.Instance.CommonTexts.Cancel);
	}

	private static void CloseLobby()
	{
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyClose();
		});
	}

	private static bool LocalTest([CanBeNull] this Entity unit, out bool isMine)
	{
		isMine = false;
		return false;
	}
}
