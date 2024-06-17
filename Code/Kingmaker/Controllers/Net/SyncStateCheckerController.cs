using Kingmaker.Controllers.Interfaces;
using Kingmaker.Networking;

namespace Kingmaker.Controllers.Net;

public class SyncStateCheckerController : IControllerTick, IController, IControllerStop
{
	private readonly HashCalculator m_HashCalculator = new HashCalculator();

	private int m_StateHash;

	private bool m_HasData;

	public int StateHash
	{
		get
		{
			if (!m_HasData)
			{
				return 0;
			}
			return m_StateHash;
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Network;
	}

	void IControllerTick.Tick()
	{
		CreateNewHash();
		CheckHash();
	}

	void IControllerStop.OnStop()
	{
		m_StateHash = 0;
		m_HasData = false;
	}

	private void CreateNewHash()
	{
		if (NetworkingManager.IsActive)
		{
			m_StateHash = m_HashCalculator.GetCurrentStateHash();
			m_HasData = true;
		}
	}

	private static void CheckHash()
	{
		PlayerCommandsCollection<SynchronizedData> synchronizedData = Game.Instance.SynchronizedDataController.SynchronizedData;
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		foreach (PlayerCommands<SynchronizedData> player in synchronizedData.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				if (!command.IsEmpty)
				{
					if (!flag2)
					{
						flag2 = true;
						num = command.stateHash;
						break;
					}
					int stateHash = command.stateHash;
					flag = num != stateHash;
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			PhotonManager.Sync.HandleActorsState();
		}
	}
}
