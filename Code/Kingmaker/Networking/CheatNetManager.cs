using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Cheats;
using ExitGames.Client.Photon;
using Kingmaker.Code.Utility.Debug;
using Kingmaker.Controllers;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Pathfinding;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Networking;

public class CheatNetManager
{
	private static bool s_Initialized;

	public static void Init()
	{
		if (!s_Initialized)
		{
			s_Initialized = true;
		}
	}

	[Cheat(Name = "net_init", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetInit()
	{
		PhotonManager.NetGame.InitNetSystem();
	}

	[Cheat(Name = "net_hash", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetHash()
	{
		using StateHasherContext stateHasherContext = StateHasherContext.Request();
		HashableState obj = stateHasherContext.GetHashableState();
		Stopwatch stopwatch = new Stopwatch();
		StructHasher<HashableState>.GetHash128(ref obj).GetHashCode();
		stopwatch.Stop();
		UnityEngine.Debug.Log($"StateHash: {stopwatch.ElapsedMilliseconds}ms");
	}

	[Cheat(Name = "net_join", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatJoinNet(string roomName = null)
	{
		PhotonManager.NetGame.Join(roomName);
	}

	[Cheat(Name = "net_stop", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatStopNet()
	{
		PhotonManager.Instance.StopPlaying();
	}

	[Cheat(Name = "net_players", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatPrintPlayers()
	{
		PhotonManager.Instance.PrintPlayers();
	}

	[Cheat(Name = "net_regions", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatPrintRegions()
	{
		PhotonManager.Instance.PrintRegions();
	}

	[Cheat(Name = "net_set_region", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSetRegion(string region)
	{
		PhotonManager.NetGame.ChangeRegion(region);
	}

	[Cheat(Name = "net_invite", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatInvite()
	{
		PhotonManager.Invite.ShowInviteWindow();
	}

	[Cheat(Name = "net_desync", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatDesync(int playerIndex = -1)
	{
		if (playerIndex != -1 && NetworkingManager.LocalNetPlayer.Index != playerIndex)
		{
			PFLog.Net.Log($"[CheatDesync] [{Game.Instance.RealTimeController.CurrentNetworkTick}] Parameter={playerIndex} player={NetworkingManager.LocalNetPlayer.Index}. Ignoring...");
			return;
		}
		AbstractUnitEntity abstractUnitEntity = Game.Instance.State.AllUnits.Where((AbstractUnitEntity u) => u.IsDirectlyControllable).First((AbstractUnitEntity u) => u.IsStarship() == Game.Instance.CurrentlyLoadedArea.IsShipArea);
		float value = UnityEngine.Random.value;
		abstractUnitEntity.Position += Vector3.one * value;
		PFLog.Net.Log($"[CheatDesync] [{Game.Instance.RealTimeController.CurrentNetworkTick}] Executing with value={value.ToStrBit()}");
	}

	[Cheat(Name = "net_state", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatState()
	{
		using StateHasherContext stateHasherContext = StateHasherContext.Request();
		HashableState hashableState = stateHasherContext.GetHashableState();
		string contents = GameStateJsonSerializer.Serializer.SerializeObject(hashableState);
		string text = System.IO.Path.Combine(ApplicationPaths.persistentDataPath, "Net", "Desync");
		Directory.CreateDirectory(text);
		string text2 = System.IO.Path.Combine(text, $"{DateTime.Now.ToFileTime()}.json");
		File.WriteAllText(text2, contents);
		PFLog.Net.Error("[CHEAT] State saved to file: '" + text2 + "'");
	}

	[Cheat(Name = "thread_sleep", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatThreadSleep(int timeMs = 1)
	{
		Thread.Sleep(timeMs);
	}

	[Cheat(Name = "net_check_math", ExecutionPolicy = ExecutionPolicy.All)]
	public static void CheatCheckMath()
	{
		float num = 0.7071067f;
		float num2 = 0.7071068f;
		int num3 = 1060439282;
		int num4 = 1060439284;
		if (BitConverter.SingleToInt32Bits(num) != num3)
		{
			PFLog.Net.Error($"'a' has different bit representation {BitConverter.SingleToInt32Bits(num)}");
		}
		if (BitConverter.SingleToInt32Bits(num2) != num4)
		{
			PFLog.Net.Error($"'b' has different bit representation {BitConverter.SingleToInt32Bits(num2)}");
		}
		float num5 = num * num;
		float num6 = num2 * num2;
		float f2 = num5 + num2 * num2;
		float f3 = num * num + num6;
		float f4 = num5 + num6;
		float num7 = num * num + num2 * num2;
		PFLog.Net.Log("Check math: " + GetCheckResult(num7) + "\na=" + Format(num) + ", b=" + Format(num2) + "\na2=" + Format(num5) + ", b2=" + Format(num6) + "\nr=" + Format(num7) + ", a2b2=" + Format(f4) + ", a2bb=" + Format(f2) + ", aab2=" + Format(f3));
		static string Format(float f)
		{
			return $"{f:G9}/{BitConverter.SingleToInt32Bits(f)}";
		}
		static string GetCheckResult(float r)
		{
			int num8 = BitConverter.SingleToInt32Bits(r);
			return num8 switch
			{
				1065353216 => "Result equal to MacOs Arm", 
				1065353215 => "Result equal to PC", 
				_ => $"Unknown result, diff={1065353216 - num8}", 
			};
		}
	}

	[Cheat(Name = "net_set_open", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSetRoomOpen(bool isOpen = true)
	{
		if (PhotonManager.Instance == null || !PhotonManager.Instance.InRoom)
		{
			PFLog.Net.Error((PhotonManager.Instance == null) ? "PhotonManager.Instance is null" : "!InRoom");
			return;
		}
		PFLog.Net.Log(string.Format("[{0}] {1}->{2}", "CheatSetRoomOpen", PhotonManager.Instance.IsRoomOpen, isOpen));
		PhotonManager.Instance.IsRoomOpen = isOpen;
	}

	[Cheat(Name = "net_is_open", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatIsRoomOpen(bool isOpen = true)
	{
		if (PhotonManager.Instance == null || !PhotonManager.Instance.InRoom)
		{
			PFLog.Net.Error((PhotonManager.Instance == null) ? "PhotonManager.Instance is null" : "!InRoom");
		}
		else
		{
			PFLog.Net.Log(string.Format("[{0}] {1}", "CheatIsRoomOpen", PhotonManager.Instance.IsRoomOpen));
		}
	}

	[Cheat(Name = "net_path", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatPath()
	{
		NavGraph[] graphs = AstarPath.active.graphs;
		int i = 0;
		for (int num = graphs.TryCount(); i < num; i++)
		{
			graphs[i].GetNodes(delegate(GraphNode node)
			{
				UnityEngine.Debug.Log($"[AstarPath] {node.NodeIndex} {node.position} {node.Vector3Position}");
			});
		}
	}

	[Cheat(Name = "net_fsm", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatRunFsmTrigger(string triggerName)
	{
		if (Enum.TryParse<NetGame.Trigger>(triggerName, out var result))
		{
			PhotonManager.NetGame.CheatFireTrigger(result);
		}
		else
		{
			PFLog.Net.Error("Can't parse string '" + triggerName + "' as Trigger");
		}
	}

	[Cheat(Name = "net_test_cmd", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatTestLoadingProcessCommandsLogic(int count = 100)
	{
		NetPlayer localNetPlayer = NetworkingManager.LocalNetPlayer;
		Game.Instance.GameCommandQueue.TestLoadingProcessCommandsLogicGameCommand(count, localNetPlayer);
	}

	[Cheat(Name = "net_avatar", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSendAvatarAlwaysViaPhoton()
	{
		DataTransporter.CheatAlwaysSendAvatarViaPhoton = !DataTransporter.CheatAlwaysSendAvatarViaPhoton;
		PFLog.Net.Log("Send avatar via photon: " + (DataTransporter.CheatAlwaysSendAvatarViaPhoton ? "enabled" : "disabled"));
	}

	[Cheat(Name = "net_av_clear", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatClearAvatarCache()
	{
		PhotonManager.Player.ClearCache(PhotonManager.Instance.LocalPlayerUserId);
	}

	[Cheat(Name = "net_allow_one", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatAllowRunWithOnePlayer()
	{
		PhotonManager.Cheat.AllowRunWithOnePlayer = !PhotonManager.Cheat.AllowRunWithOnePlayer;
	}

	[Cheat(Name = "net_packet", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSetMaxPacketSize(int packetSizeKb)
	{
		SaveMetaData.MaxPacketSize = packetSizeKb * 1024;
	}

	[Cheat(Name = "net_slow", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSlow(bool activate = true)
	{
		if (activate)
		{
			TimeSpeedController.ForceSpeedMode(8);
		}
		else
		{
			TimeSpeedController.ForceSpeedMode();
		}
	}

	[Cheat(Name = "net_sim", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetIncomingLag(bool enabled, bool reset = false)
	{
		PhotonManager.Instance.IsSimulationEnabled = enabled;
		if (reset)
		{
			PhotonManager.Instance.SimulationSettings.IncomingLag = 0;
			PhotonManager.Instance.SimulationSettings.OutgoingLag = 0;
			PhotonManager.Instance.SimulationSettings.IncomingJitter = 0;
			PhotonManager.Instance.SimulationSettings.OutgoingJitter = 0;
			PhotonManager.Instance.SimulationSettings.IncomingLossPercentage = 0;
			PhotonManager.Instance.SimulationSettings.OutgoingLossPercentage = 0;
		}
		PFLog.Net.Log($"Cur settings: {PhotonManager.Instance.SimulationSettings}");
	}

	[Cheat(Name = "net_sim_lag", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetLag(int lag)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			int incomingLag = (sim.OutgoingLag = lag);
			sim.IncomingLag = incomingLag;
		});
	}

	[Cheat(Name = "net_sim_inlag", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetIncomingLag(int lag)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.IncomingLag = lag;
		});
	}

	[Cheat(Name = "net_sim_outlag", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetOutgoingLag(int lag)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.OutgoingLag = lag;
		});
	}

	[Cheat(Name = "net_sim_jit", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetJitter(int jit)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			int incomingJitter = (sim.OutgoingJitter = jit);
			sim.IncomingJitter = incomingJitter;
		});
	}

	[Cheat(Name = "net_sim_injit", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetIncomingJitter(int jit)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.IncomingJitter = jit;
		});
	}

	[Cheat(Name = "net_sim_outjit", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetOutgoingJitter(int jit)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.OutgoingJitter = jit;
		});
	}

	[Cheat(Name = "net_sim_loss", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetLossPercentage(int loss)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			int incomingLossPercentage = (sim.OutgoingLossPercentage = loss);
			sim.IncomingLossPercentage = incomingLossPercentage;
		});
	}

	[Cheat(Name = "net_sim_inloss", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetIncomingLossPercentage(int loss)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.IncomingLossPercentage = loss;
		});
	}

	[Cheat(Name = "net_sim_outloss", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatNetSimulationSetOutgoingLossPercentage(int loss)
	{
		SetSimulation(delegate(NetworkSimulationSet sim)
		{
			sim.OutgoingLossPercentage = loss;
		});
	}

	private static void SetSimulation(Action<NetworkSimulationSet> set)
	{
		if (PhotonManager.Instance != null)
		{
			PhotonManager.Instance.IsSimulationEnabled = true;
			set(PhotonManager.Instance.SimulationSettings);
			PFLog.Net.Log($"Cur settings: {PhotonManager.Instance.SimulationSettings}");
		}
		else
		{
			PFLog.Net.Error("[CheatNetManager.SetSimulation] Photon not initialized");
		}
	}
}
