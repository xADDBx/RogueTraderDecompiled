using System;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.Networking.Tools;
using Kingmaker.Stores;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CircularBuffer;
using Owlcat.Core.Overlays;
using UnityEngine;

namespace Kingmaker.QA.Overlays;

public class NetworkingOverlay : Overlay
{
	private const int TicksPerSecond = 20;

	private const int HistoryLength = 60;

	private static readonly CircularBuffer<int> SkipCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> SentGameCommandCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> SentUnitCommandCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> ReceivedGameCommandCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> ReceivedUnitCommandCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> SentBytesCount = new CircularBuffer<int>(60);

	private static readonly CircularBuffer<int> ReceivedBytesCount = new CircularBuffer<int>(60);

	public NetworkingOverlay()
		: base("Networking", CreateElements())
	{
		NewTick();
	}

	public static void NewTick()
	{
		SkipCount.Append(0);
	}

	public static void AddSkipTick()
	{
		SkipCount.Last++;
	}

	public static void AddCommands(int gameCommandCount, int unitCommandCount)
	{
		SentGameCommandCount.Append(gameCommandCount);
		SentUnitCommandCount.Append(unitCommandCount);
	}

	public static void AddReceivedCommands(Type type, int count)
	{
		if (type == typeof(GameCommand))
		{
			ReceivedGameCommandCount.Append(count);
		}
		else if (type == typeof(UnitCommandParams))
		{
			ReceivedUnitCommandCount.Append(count);
		}
	}

	public static void AddSentBytes(int n)
	{
		SentBytesCount.Append(n);
	}

	public static void AddReceivedBytes(int n)
	{
		ReceivedBytesCount.Append(n);
	}

	private static OverlayElement[] CreateElements()
	{
		int min6;
		float med6;
		int max6;
		int min5;
		float med5;
		int max5;
		int min4;
		float med4;
		int max4;
		int min3;
		float med3;
		int max3;
		int min2;
		float med2;
		int max2;
		int min;
		float med;
		int max;
		return new OverlayElement[25]
		{
			new Label("Strategy", () => PhotonManager.Sync.StrategyType.Name),
			new Label("Store", delegate
			{
				if (StoreManager.Store != StoreType.Steam)
				{
					return StoreManager.Store.ToString();
				}
				return (!SteamManager.Initialized) ? "Steam (NOT initialized)" : "Steam";
			}),
			new Label("Server", () => (!PhotonManager.Initialized) ? string.Empty : (PhotonManager.Instance.Region ?? PhotonManager.Lobby.CurrentState.ToString())),
			new Label("Net Game", () => (!PhotonManager.Initialized) ? string.Empty : PhotonManager.Lobby.CurrentState.ToString()),
			new Label("FSM State", () => PhotonManager.NetGame.CurrentState.ToString()),
			new Label("Network Tick", () => (Game.Instance.Player == null) ? "--" : Game.Instance.RealTimeController.CurrentNetworkTick.ToString()),
			new Label("Skip Count", delegate
			{
				int count = SkipCount.Count;
				if (count == 0)
				{
					return "--";
				}
				int num = int.MaxValue;
				float num2 = 0f;
				int num3 = int.MinValue;
				for (int i = 0; i < count; i++)
				{
					int num4 = SkipCount[i];
					num = Mathf.Min(num, num4);
					num2 += (float)num4;
					num3 = Mathf.Max(num3, num4);
				}
				num2 /= (float)count;
				return $"{num} | {num2:0.0} | {num3}";
			}),
			new Label("Lag", delegate
			{
				int minLag = Game.Instance.TimeSpeedController.GetMinLag(byte.MaxValue);
				int maxLag = Game.Instance.TimeSpeedController.MaxLag;
				int minLag2 = Game.Instance.SynchronizedDataController.SynchronizedData.GetMinLag(255);
				int maxLag2 = Game.Instance.SynchronizedDataController.SynchronizedData.GetMaxLag();
				return $"{minLag}:{maxLag} | {minLag2}:{maxLag2}";
			}),
			new Label("Speed Mode", delegate
			{
				int currentSpeedMode = Game.Instance.TimeSpeedController.CurrentSpeedMode;
				return $"{currentSpeedMode} | {Game.Instance.RealTimeController.TimeScale}";
			}),
			new Label("Id", () => $"{NetworkingManager.LocalNetPlayer.Index}/{NetworkingManager.PlayersCount}"),
			new Label("Sync", () => (Game.Instance.Player != null && PhotonManager.Initialized) ? ((!PhotonManager.Sync.HasDesync) ? ((!PhotonManager.Sync.WasDesync) ? "ok" : "OK, but it was") : "DESYNC") : "--"),
			new Label("Room Name", () => (!PhotonManager.Initialized || string.IsNullOrEmpty(PhotonManager.Instance.RoomName)) ? "--" : (PhotonManager.Instance.RoomName + " (" + (PhotonManager.Instance.IsRoomOpen ? "op" : "CL") + ")")),
			new Label("↑ Game cmd", () => (!GetStats(SentGameCommandCount, out min6, out med6, out max6)) ? "--" : $"{min6} | {med6:0.0} | {max6}"),
			new Label("↓ Game cmd", () => (!GetStats(ReceivedGameCommandCount, out min5, out med5, out max5)) ? "--" : $"{min5} | {med5:0.0} | {max5}"),
			new Label("↑ Unit cmd", () => (!GetStats(SentUnitCommandCount, out min4, out med4, out max4)) ? "--" : $"{min4} | {med4:0.0} | {max4}"),
			new Label("↓ Unit cmd", () => (!GetStats(ReceivedUnitCommandCount, out min3, out med3, out max3)) ? "--" : $"{min3} | {med3:0.0} | {max3}"),
			new Label("↑ KB", () => (!GetStats(SentBytesCount, out min2, out med2, out max2)) ? "--" : $"{(float)min2 / 1024f:0.00} | {med2 / 1024f:0.00} | {(float)max2 / 1024f:0.00}"),
			new Label("↓ KB", () => (!GetStats(ReceivedBytesCount, out min, out med, out max)) ? "--" : $"{(float)min / 1024f:0.00} | {med / 1024f:0.00} | {(float)max / 1024f:0.00}"),
			new Label("↑ Photon", delegate
			{
				if (PhotonManager.Initialized)
				{
					PhotonTrafficStats photonTrafficStats2 = PhotonManager.Instance.PhotonTrafficStats;
					return ValueWithUnits(photonTrafficStats2.SendBytes) + " | " + ValueWithUnits(photonTrafficStats2.SendBytesPerSec) + "/sec";
				}
				return "--";
			}),
			new Label("↓ Photon", delegate
			{
				if (PhotonManager.Initialized)
				{
					PhotonTrafficStats photonTrafficStats = PhotonManager.Instance.PhotonTrafficStats;
					return ValueWithUnits(photonTrafficStats.ReceiveBytes) + " | " + ValueWithUnits(photonTrafficStats.ReceiveBytesPerSec) + "/sec";
				}
				return "--";
			}),
			new Label("Rtt", delegate
			{
				if (PhotonManager.Instance != null)
				{
					PhotonNetworkStats photonNetworkStats2 = PhotonManager.Instance.PhotonNetworkStats;
					return $"{photonNetworkStats2.Rtt:D3} | {photonNetworkStats2.RttVariance:D3}";
				}
				return "--";
			}),
			new Label("Last recv", () => (!(PhotonManager.Instance != null)) ? "--" : $"{PhotonManager.Instance.PhotonNetworkStats.LastSocketReceive:D5}"),
			new Label("Max delta send/recv", delegate
			{
				if (PhotonManager.Instance != null)
				{
					PhotonNetworkStats photonNetworkStats = PhotonManager.Instance.PhotonNetworkStats;
					return $"{photonNetworkStats.LongestDeltaBetweenSending:D3} | {photonNetworkStats.LongestDeltaBetweenDispatching:D3}";
				}
				return "--";
			}),
			new Label("OpStats", () => (!(PhotonManager.Instance != null)) ? "--" : PhotonManager.Instance.PhotonNetworkStats.GetLongestOperationsStats()),
			new Label("Prediction", () => Game.Instance.IsControllerGamepad ? ((!Game.Instance.MovePredictionController.IsActive) ? "off" : "ON") : "off (PC)")
		};
	}

	private static bool GetStats(CircularBuffer<int> buffer, out int min, out float med, out int max)
	{
		min = int.MaxValue;
		med = 0f;
		max = int.MinValue;
		int count = buffer.Count;
		if (count == 0)
		{
			return false;
		}
		for (int i = 0; i < count; i++)
		{
			int num = buffer[i];
			min = Mathf.Min(min, num);
			med += num;
			max = Mathf.Max(max, num);
		}
		med /= count;
		return true;
	}

	private static string ValueWithUnits(int value)
	{
		if (value > 10485760)
		{
			return $"{value / 1048576} MB";
		}
		if (value > 10240)
		{
			return $"{value / 1024} KB";
		}
		return $"{value} B";
	}
}
