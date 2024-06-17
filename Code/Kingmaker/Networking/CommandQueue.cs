using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Networking;

public class CommandQueue
{
	public const int TickDelay = 1;
}
public class CommandQueue<T> : CommandQueue
{
	private class CommandsForStep
	{
		public readonly PlayerCommandsCollection<T> Commands = new PlayerCommandsCollection<T>();

		public int TickIndex = -1;

		public NetPlayerGroup ReadyPlayerGroup;

		public bool IsEmpty
		{
			get
			{
				if (TickIndex == -1)
				{
					return ReadyPlayerGroup.IsEmpty;
				}
				return false;
			}
		}

		public int CommandsCount
		{
			get
			{
				int num = 0;
				foreach (PlayerCommands<T> player in Commands.Players)
				{
					num += player.Count;
				}
				return num;
			}
		}

		public void Init(int tickIndex)
		{
			if (!IsEmpty)
			{
				throw new Exception($"Attempt to rewrite non-empty command element! n={tickIndex} e={TickIndex} c={Game.Instance.RealTimeController.CurrentNetworkTick}");
			}
			TickIndex = tickIndex;
		}

		public void Reset()
		{
			Commands.Clear();
			TickIndex = -1;
			ReadyPlayerGroup = NetPlayerGroup.Empty;
		}

		public bool IsReady(NetPlayerGroup targetPlayerGroup)
		{
			return ReadyPlayerGroup.Contains(targetPlayerGroup);
		}

		public void SetPlayersReadyBit(NetPlayer player, int tickIndex)
		{
			if (ReadyPlayerGroup.Contains(player))
			{
				PFLog.Net.Error($"PlayersReadyBit is already set! Player#{player.ToString()} Tick#{tickIndex} Players={ReadyPlayerGroup.ToString()}");
			}
			ReadyPlayerGroup = ReadyPlayerGroup.Add(player);
		}
	}

	public interface ICommandSender
	{
		void SendCommands(int lockStepIndex, List<T> commands);
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("CommandQueue<" + typeof(T).Name + ">");

	private readonly List<T> m_CurrentTickCommands = new List<T>();

	private const int CommandsForStepLength = 18;

	private readonly CommandsForStep[] m_CommandsForStep = new CommandsForStep[18];

	private int m_NextQueueIndex;

	private int m_LastTickIndex = -1;

	private int m_SkipUntilTick;

	private int m_LastSendTickIndex = -1;

	private static NetPlayerGroup PlayersReadyMask => NetworkingManager.PlayersReadyMask;

	private static NetPlayer LocalNetPlayer => NetworkingManager.LocalNetPlayer;

	public bool HasScheduledCommands => m_CommandsForStep.Any((CommandsForStep x) => !x.IsEmpty && 0 < x.CommandsCount);

	public CommandQueue()
	{
		int i = 0;
		for (int num = m_CommandsForStep.Length; i < num; i++)
		{
			m_CommandsForStep[i] = new CommandsForStep();
		}
	}

	public void PushCommandsForPlayer(NetPlayer player, int tickIndex, IEnumerable<T> commands)
	{
		if (commands == null)
		{
			commands = Enumerable.Empty<T>();
		}
		GetCommandsByStep(tickIndex, out var commandsForStep, create: true);
		commandsForStep.Commands.AddCommands(player, commands);
		commandsForStep.SetPlayersReadyBit(player, tickIndex);
	}

	public void CancelCurrentCommands()
	{
		int num = m_LastSendTickIndex + 1;
		Logger.Log($"SkipUntilTick {m_SkipUntilTick} -> {num}");
		m_SkipUntilTick = num;
	}

	public void Reset()
	{
		m_CurrentTickCommands.Clear();
		int i = 0;
		for (int num = m_CommandsForStep.Length; i < num; i++)
		{
			m_CommandsForStep[i].Reset();
		}
		m_NextQueueIndex = 0;
		m_LastTickIndex = -1;
		m_SkipUntilTick = 0;
		m_LastSendTickIndex = -1;
	}

	public bool IsReady(int tickIndex)
	{
		if (IsAlreadyProcessed(tickIndex))
		{
			return true;
		}
		if (GetCommandsByStep(tickIndex, out var commandsForStep))
		{
			return commandsForStep.IsReady(PlayersReadyMask);
		}
		return false;
	}

	public int GetMaxReadyTick()
	{
		return GetMaxReadyTick(PlayersReadyMask);
	}

	public int GetMaxReadyTick(NetPlayerGroup playerGroup)
	{
		int num = m_LastTickIndex;
		bool flag = true;
		while (flag)
		{
			num++;
			flag = GetCommandsByStep(num, out var commandsForStep) && commandsForStep.IsReady(playerGroup);
		}
		return num - 1;
	}

	public void SaveCommands(List<T> commands, bool clear = true)
	{
		m_CurrentTickCommands.AddRange(commands);
		if (clear)
		{
			commands.Clear();
		}
	}

	public void PrepareForSend(int sendTickIndex, List<T> commands)
	{
		m_LastSendTickIndex = sendTickIndex;
		commands.AddRange(m_CurrentTickCommands);
		PushCommandsForPlayer(LocalNetPlayer, sendTickIndex, m_CurrentTickCommands);
		m_CurrentTickCommands.Clear();
	}

	public bool LoadCommands(PlayerCommandsCollection<T> unitCommandsInProcess, bool onlyOneTick = false)
	{
		int num = Game.Instance.TimeSpeedController.GetTickIndex(Game.Instance.RealTimeController.CurrentNetworkTick);
		if (IsAlreadyProcessed(num))
		{
			return false;
		}
		int num2 = ((m_LastTickIndex == -1) ? num : (m_LastTickIndex + 1));
		if (onlyOneTick)
		{
			num = num2;
			unitCommandsInProcess.Clear();
		}
		for (int i = num2; i <= num; i++)
		{
			LoadCommands(unitCommandsInProcess, i);
		}
		m_LastTickIndex = num;
		return true;
	}

	private void LoadCommands(PlayerCommandsCollection<T> unitCommandsInProcess, int tickIndex)
	{
		if (GetCommandsByStep(tickIndex, out var commandsForStep))
		{
			if (!commandsForStep.IsReady(PlayersReadyMask))
			{
				PFLog.Net.Error($"CommandQueue<{typeof(T).Name}>.LoadCommands: commands are not ready! Expectation={PlayersReadyMask.ToString()} Players={commandsForStep.ReadyPlayerGroup.ToString()} tickIndex={tickIndex}");
			}
			if (tickIndex >= m_SkipUntilTick)
			{
				int num = 0;
				int i = 0;
				for (int count = commandsForStep.Commands.Players.Count; i < count; i++)
				{
					PlayerCommands<T> playerCommands = commandsForStep.Commands.Players[i];
					if (playerCommands.Commands.Count != 0)
					{
						unitCommandsInProcess.AddCommands(playerCommands.Player, playerCommands.Commands);
						num += ((!LocalNetPlayer.Equals(playerCommands.Player)) ? playerCommands.Count : 0);
					}
				}
			}
			commandsForStep.Reset();
		}
		else
		{
			PFLog.Net.Error($"CommandQueue<{typeof(T).Name}>.LoadCommands: data not found! TickIndex={tickIndex}");
		}
	}

	private bool GetCommandsByStep(int tickIndex, out CommandsForStep commandsForStep, bool create = false)
	{
		int i = 0;
		for (int num = m_CommandsForStep.Length; i < num; i++)
		{
			CommandsForStep commandsForStep2 = m_CommandsForStep[i];
			if (commandsForStep2.TickIndex == tickIndex)
			{
				commandsForStep = commandsForStep2;
				return true;
			}
		}
		if (create)
		{
			CommandsForStep commandsForStep3 = m_CommandsForStep[m_NextQueueIndex];
			m_NextQueueIndex = (m_NextQueueIndex + 1) % m_CommandsForStep.Length;
			commandsForStep3.Init(tickIndex);
			commandsForStep = commandsForStep3;
			return true;
		}
		commandsForStep = null;
		return false;
	}

	public void DumpScheduledCommands()
	{
		if (0 < m_CurrentTickCommands.Count)
		{
			Logger.Warning($"x{m_CurrentTickCommands.Count} {typeof(T).Name} (s) are still in curr queue:");
			int i = 0;
			for (int count = m_CurrentTickCommands.Count; i < count; i++)
			{
				Logger.Warning($"  {i}) {m_CurrentTickCommands[i].ToString()}");
			}
		}
		CommandsForStep[] commandsForStep = m_CommandsForStep;
		foreach (CommandsForStep commandsForStep2 in commandsForStep)
		{
			if (commandsForStep2.TickIndex == -1 || commandsForStep2.CommandsCount == 0)
			{
				continue;
			}
			Logger.Warning($"x{commandsForStep2.Commands.Players.Count} {typeof(T).Name} (s) are still in queue for tick#{commandsForStep2.TickIndex}:");
			int k = 0;
			for (int count2 = commandsForStep2.Commands.Players.Count; k < count2; k++)
			{
				Logger.Warning($" P#{k}: x{commandsForStep2.Commands.Players[k].Count}");
				int l = 0;
				for (int count3 = commandsForStep2.Commands.Players[k].Count; l < count3; l++)
				{
					Logger.Warning($"  {l}) {commandsForStep2.Commands.Players[k][l].ToString()}");
				}
			}
		}
	}

	private bool IsAlreadyProcessed(int tickIndex)
	{
		bool flag = tickIndex <= m_LastTickIndex;
		if (flag)
		{
			int i = 0;
			for (int num = m_CommandsForStep.Length; i < num; i++)
			{
				if (m_CommandsForStep[i].TickIndex == tickIndex)
				{
					throw new Exception($"CommandQueue<{typeof(T).Name}>.IsReady({tickIndex}): The processed element was not removed from the queue!");
				}
			}
		}
		return flag;
	}

	[Conditional("FALSE")]
	private static void CheckDuplicates(CommandsForStep commandsForStep)
	{
	}
}
