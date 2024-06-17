using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.Replay;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.GameCommands;

public class GameCommandQueue
{
	public interface IInvokable
	{
		void Invoke();
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("GameCommandQueue");

	private readonly CommandQueue<GameCommand> m_CommandQueue = new CommandQueue<GameCommand>();

	private readonly List<(NetPlayer, GameCommand)> m_ScheduledCommands = new List<(NetPlayer, GameCommand)>();

	private readonly List<GameCommand> m_ScheduledCommandsForFuture = new List<GameCommand>();

	private readonly List<GameCommand> m_TempCommandsList = new List<GameCommand>();

	private readonly PlayerCommandsCollection<GameCommand> m_ScheduledCommandsCache = new PlayerCommandsCollection<GameCommand>();

	public int Count
	{
		get
		{
			lock (m_ScheduledCommands)
			{
				return m_ScheduledCommands.Count;
			}
		}
	}

	public bool HasScheduledCommands
	{
		get
		{
			if (Count <= 0)
			{
				return m_CommandQueue.HasScheduledCommands;
			}
			return true;
		}
	}

	public void CancelCurrentCommands()
	{
		lock (m_ScheduledCommands)
		{
			m_ScheduledCommands.Clear();
		}
		m_ScheduledCommandsForFuture.Clear();
		m_CommandQueue.CancelCurrentCommands();
	}

	public void Clear()
	{
		m_CommandQueue.Reset();
		lock (m_ScheduledCommands)
		{
			m_ScheduledCommands.Clear();
		}
	}

	public void PushCommandsForPlayer(NetPlayer playerIndex, int tickIndex, IEnumerable<GameCommand> commands)
	{
		m_CommandQueue.PushCommandsForPlayer(playerIndex, tickIndex, commands);
	}

	public void PrepareForSend(int sendTickIndex, List<GameCommand> gameCommands)
	{
		m_CommandQueue.PrepareForSend(sendTickIndex, gameCommands);
	}

	public bool IsReady(int tickIndex)
	{
		return m_CommandQueue.IsReady(tickIndex);
	}

	public int GetMaxReadyTick()
	{
		return m_CommandQueue.GetMaxReadyTick();
	}

	public void AddCommand([NotNull] GameCommand cmd)
	{
		if (((bool)ContextData<GameCommandContext>.Current || (bool)ContextData<UnitCommandContext>.Current) && cmd.IsSynchronized && !cmd.IsForcedSynced)
		{
			throw new PlayerCommandInsideEffectContextException(cmd.GetType().ToString());
		}
		if ((bool)ContextData<GameCommandContext>.Current && cmd.IsSynchronized && cmd.IsForcedSynced)
		{
			m_ScheduledCommandsForFuture.Add(cmd);
			return;
		}
		lock (m_ScheduledCommands)
		{
			m_ScheduledCommands.Add((NetPlayer.Empty, cmd));
		}
	}

	public bool ContainsCommand<T>([CanBeNull] Func<T, bool> filter = null) where T : GameCommand
	{
		lock (m_ScheduledCommands)
		{
			int i = 0;
			for (int count = m_ScheduledCommands.Count; i < count; i++)
			{
				if (m_ScheduledCommands[i].Item2 is T arg && (filter == null || filter(arg)))
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Tick()
	{
		using (ProfileScope.New("GameCommandQueue.Tick"))
		{
			using (ContextData<GameCommandContext>.Request())
			{
				if (!Game.Instance.RealTimeController.IsSystemTick)
				{
					return;
				}
				lock (m_ScheduledCommands)
				{
					Kingmaker.Replay.Replay.SaveGameCommands(m_ScheduledCommands);
					Kingmaker.Replay.Replay.LoadGameCommands(m_ScheduledCommands);
					SaveCommands(m_ScheduledCommands);
					LoadCommands(m_ScheduledCommands);
					for (int i = 0; i < m_ScheduledCommands.Count; i++)
					{
						NetPlayer item = m_ScheduledCommands[i].Item1;
						GameCommand item2 = m_ScheduledCommands[i].Item2;
						using (ContextData<GameCommandPlayer>.Request().Setup(item2, item))
						{
							using (ProfileScope.New(TypesCache.GetTypeName(item2.GetType())))
							{
								item2.Execute();
							}
						}
					}
					m_ScheduledCommands.Clear();
					foreach (GameCommand item3 in m_ScheduledCommandsForFuture)
					{
						m_ScheduledCommands.Add((NetPlayer.Empty, item3));
					}
					m_ScheduledCommandsForFuture.Clear();
				}
			}
		}
	}

	public void LockAndRun<T>(T action) where T : struct, IInvokable
	{
		lock (m_ScheduledCommands)
		{
			action.Invoke();
		}
	}

	private void SaveCommands(List<(NetPlayer, GameCommand)> gameCommands)
	{
		List<GameCommand> tempCommandsList = m_TempCommandsList;
		tempCommandsList.IncreaseCapacity(gameCommands.Count);
		int i = 0;
		for (int num = gameCommands.Count; i < num; i++)
		{
			GameCommand item = gameCommands[i].Item2;
			if (item.IsSynchronized)
			{
				tempCommandsList.Add(item);
				gameCommands.RemoveAt(i);
				i--;
				num--;
			}
		}
		m_CommandQueue.SaveCommands(tempCommandsList);
	}

	private void LoadCommands(List<(NetPlayer, GameCommand)> gameCommands)
	{
		m_ScheduledCommandsCache.Clear();
		m_CommandQueue.LoadCommands(m_ScheduledCommandsCache);
		m_ScheduledCommandsCache.Fill(gameCommands);
	}

	public void DumpScheduledCommands()
	{
		lock (m_ScheduledCommands)
		{
			foreach (var scheduledCommand in m_ScheduledCommands)
			{
				Logger.Warning("Command {0} is still in queue during loading", scheduledCommand);
			}
		}
		m_CommandQueue.DumpScheduledCommands();
	}
}
