using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Player;
using Kingmaker.QA.Overlays;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine.Pool;

namespace Kingmaker.Networking;

public static class NetworkingManager
{
	private static readonly List<NetPlayer> m_ActivePlayers = new List<NetPlayer>(8);

	public static bool IsEnabled => true;

	public static bool IsActive => PhotonManager.Lobby.IsPlaying;

	public static int PlayersCount
	{
		get
		{
			if (!PhotonManager.Initialized)
			{
				return 1;
			}
			return PhotonManager.Instance.PlayerCount;
		}
	}

	public static bool IsMultiplayer => 1 < PlayersCount;

	public static NetPlayerGroup PlayersReadyMask
	{
		get
		{
			if (!PhotonManager.Initialized)
			{
				return NetPlayerGroup.Offline;
			}
			return PhotonManager.Instance.PlayersReadyMask;
		}
	}

	public static NetPlayer LocalNetPlayer
	{
		get
		{
			if (!PhotonManager.Initialized)
			{
				return NetPlayer.Offline;
			}
			return PhotonManager.Instance.LocalNetPlayer;
		}
	}

	public static NetPlayer GameOwner
	{
		get
		{
			if (!PhotonManager.Initialized)
			{
				return NetPlayer.Offline;
			}
			return PhotonManager.Instance.RoomOwner;
		}
	}

	public static bool IsGameOwner => LocalNetPlayer.Equals(GameOwner);

	public static ReadonlyList<NetPlayer> ActivePlayers
	{
		get
		{
			m_ActivePlayers.Clear();
			if (PhotonManager.Initialized)
			{
				foreach (PlayerInfo activePlayer in PhotonManager.Instance.ActivePlayers)
				{
					m_ActivePlayers.Add(activePlayer.NetPlayer);
				}
			}
			if (m_ActivePlayers.Count == 0)
			{
				m_ActivePlayers.Add(NetPlayer.Offline);
			}
			return m_ActivePlayers;
		}
	}

	public static void ReceivePackets()
	{
		if (PhotonManager.Initialized)
		{
			PhotonManager.Instance.Receive();
		}
	}

	public static void SendPackets(int sendTickIndex)
	{
		List<GameCommand> value;
		using (CollectionPool<List<GameCommand>, GameCommand>.Get(out value))
		{
			List<UnitCommandParams> value2;
			using (CollectionPool<List<UnitCommandParams>, UnitCommandParams>.Get(out value2))
			{
				List<SynchronizedData> value3;
				using (CollectionPool<List<SynchronizedData>, SynchronizedData>.Get(out value3))
				{
					Game.Instance.GameCommandQueue.PrepareForSend(sendTickIndex, value);
					Game.Instance.UnitCommandBuffer.PrepareForSend(sendTickIndex, value2);
					Game.Instance.SynchronizedDataController.PrepareForSend(sendTickIndex, value3);
					NetworkingOverlay.AddCommands(value.Count, value2.Count);
					if (PhotonManager.NetGame.CanSendGameMessages())
					{
						PhotonManager.Command.SendAllCommands(sendTickIndex, value, value2, value3);
						PhotonManager.Instance.Send();
					}
				}
			}
		}
	}

	public static bool LockOn(NetLockPointId pointId)
	{
		return PhotonManager.Lock.Lock(pointId);
	}

	public static TimeSpan ProcessDeltaTime(TimeSpan time, TimeSpan deltaTime)
	{
		if (!IsActive)
		{
			return deltaTime;
		}
		return PhotonManager.Instance.ProcessDeltaTime(time, deltaTime);
	}

	public static PhotonActorNumber PlayerToActorNumber(NetPlayer player)
	{
		if (!IsActive)
		{
			return PhotonActorNumber.Invalid;
		}
		return PhotonManager.Instance.PlayerToActorNumber(player);
	}

	public static int ActorNumberToPlayerIndex(int actorNumber)
	{
		if (!IsActive)
		{
			return -1;
		}
		return PhotonManager.Instance.ActorNumberToPlayerIndex(actorNumber);
	}

	public static bool GetNickName(NetPlayer player, out string nickName)
	{
		if (!IsActive)
		{
			nickName = null;
			return false;
		}
		return PhotonManager.Player.GetNickName(player, out nickName);
	}
}
