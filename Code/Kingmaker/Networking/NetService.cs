using System;
using System.Collections.Generic;
using Kingmaker.Controllers.Net;
using Kingmaker.GameCommands;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine.Pool;

namespace Kingmaker.Networking;

public class NetService : IService, IDisposable
{
	private static ServiceProxy<NetService> s_InstanceProxy;

	private bool m_Initialized;

	public static NetService Instance
	{
		get
		{
			if (s_InstanceProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new NetService());
				s_InstanceProxy = Services.GetProxy<NetService>();
			}
			return s_InstanceProxy.Instance;
		}
	}

	public bool Initialized => m_Initialized;

	private static int CurrentTickIndex => Game.Instance.RealTimeController.CurrentNetworkTick;

	ServiceLifetimeType IService.Lifetime => ServiceLifetimeType.GameSession;

	public void Init()
	{
		if (m_Initialized)
		{
			return;
		}
		m_Initialized = true;
		int num = CurrentTickIndex + 1;
		ReadonlyList<NetPlayer> activePlayers = NetworkingManager.ActivePlayers;
		int i = 0;
		for (int count = activePlayers.Count; i < count; i++)
		{
			NetPlayer netPlayer = activePlayers[i];
			List<GameCommand> value;
			using (CollectionPool<List<GameCommand>, GameCommand>.Get(out value))
			{
				List<UnitCommandParams> value2;
				using (CollectionPool<List<UnitCommandParams>, UnitCommandParams>.Get(out value2))
				{
					List<SynchronizedData> value3;
					using (CollectionPool<List<SynchronizedData>, SynchronizedData>.Get(out value3))
					{
						Game.Instance.GameCommandQueue.PushCommandsForPlayer(netPlayer, num, value);
						Game.Instance.UnitCommandBuffer.PushCommandsForPlayer(netPlayer, num, value2);
						Game.Instance.SynchronizedDataController.PushDataForPlayer(netPlayer, num, value3);
					}
				}
			}
		}
		for (int j = 1; j < 1; j++)
		{
			NetworkingManager.SendPackets(num + j);
		}
	}

	public void Tick()
	{
		NetworkingManager.SendPackets(CurrentTickIndex + 1);
	}

	public void CancelCurrentCommands()
	{
		PathfindingService.Instance.ForceCompleteAll();
		PFLog.GameCommands.Log($"Canceling current commands... tick#{Game.Instance.RealTimeController.CurrentNetworkTick}");
		Game.Instance.UnitCommandBuffer.CancelCurrentCommands();
		Game.Instance.AbilityExecutor.DetachAll();
	}

	void IDisposable.Dispose()
	{
		m_Initialized = false;
		Game.Instance.GameCommandQueue.DumpScheduledCommands();
		Game.Instance.GameCommandQueue.Clear();
		Game.Instance.UnitCommandBuffer.DumpScheduledCommands();
		Game.Instance.UnitCommandBuffer.Clear();
		Game.Instance.TimeSpeedController.Clear();
		if (PhotonManager.Instance != null)
		{
			PhotonManager.Instance.Reset();
		}
		PhotonManager.Sync.Reset();
	}
}
