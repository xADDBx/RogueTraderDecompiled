using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.Replay;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Controllers.Units;

public class UnitCommandBuffer : IControllerTick, IController, IControllerStart, IControllerStop
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitCommandBuffer");

	private readonly CommandQueue<UnitCommandParams> m_CommandQueue = new CommandQueue<UnitCommandParams>();

	private List<UnitCommandParams> m_UnitCommands = new List<UnitCommandParams>();

	private List<UnitCommandParams> m_UnitCommandsInProcess = new List<UnitCommandParams>();

	private readonly PlayerCommandsCollection<UnitCommandParams> m_UnitCommandsInProcessCache = new PlayerCommandsCollection<UnitCommandParams>();

	public void PushCommandsForPlayer(NetPlayer playerIndex, int tickIndex, IEnumerable<UnitCommandParams> commands)
	{
		m_CommandQueue.PushCommandsForPlayer(playerIndex, tickIndex, commands);
	}

	public void PrepareForSend(int sendTickIndex, List<UnitCommandParams> unitCommands)
	{
		m_CommandQueue.PrepareForSend(sendTickIndex, unitCommands);
	}

	public bool IsReady(int tickIndex)
	{
		return m_CommandQueue.IsReady(tickIndex);
	}

	public int GetMaxReadyTick()
	{
		return m_CommandQueue.GetMaxReadyTick();
	}

	public int GetMaxReadyTick(NetPlayerGroup playerGroup)
	{
		return m_CommandQueue.GetMaxReadyTick(playerGroup);
	}

	public bool TryAdd([NotNull] Entity owner, [NotNull] UnitCommandParams unitCommand)
	{
		return TryAddInternal(owner, unitCommand, UnitCommandParams.CommandType.Run);
	}

	public bool TryAddToQueue([NotNull] Entity owner, [NotNull] UnitCommandParams unitCommand)
	{
		return TryAddInternal(owner, unitCommand, UnitCommandParams.CommandType.AddToQueue);
	}

	public bool TryAddToQueueFirst([NotNull] Entity owner, [NotNull] UnitCommandParams unitCommand)
	{
		return TryAddInternal(owner, unitCommand, UnitCommandParams.CommandType.AddToQueueFirst);
	}

	private bool TryAddInternal([NotNull] Entity owner, [NotNull] UnitCommandParams unitCommand, UnitCommandParams.CommandType commandType)
	{
		if (((bool)ContextData<GameCommandContext>.Current || (bool)ContextData<UnitCommandContext>.Current) && unitCommand.IsSynchronized)
		{
			throw new PlayerCommandInsideEffectContextException(unitCommand.GetType().ToString());
		}
		if (unitCommand.IsSynchronized)
		{
			if (unitCommand.GetType() == typeof(UnitUseAbilityParams))
			{
				throw new Exception("UnitUseAbility can't be IsPlayerCommand! Consider to use PlayerUseAbility instead");
			}
			unitCommand.Type = commandType;
			unitCommand.OwnerRef = new EntityRef<BaseUnitEntity>(owner.UniqueId);
			m_UnitCommands.Add(unitCommand);
			return true;
		}
		return false;
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		using (ContextData<UnitCommandContext>.Request())
		{
			List<UnitCommandParams> unitCommandsInProcess = m_UnitCommandsInProcess;
			List<UnitCommandParams> unitCommands = m_UnitCommands;
			m_UnitCommands = unitCommandsInProcess;
			m_UnitCommandsInProcess = unitCommands;
			Kingmaker.Replay.Replay.SaveUnitCommandsInProcess(m_UnitCommandsInProcess);
			Kingmaker.Replay.Replay.LoadUnitCommandsInProcess(m_UnitCommandsInProcess);
			m_CommandQueue.SaveCommands(m_UnitCommandsInProcess);
			m_UnitCommandsInProcessCache.Clear();
			m_CommandQueue.LoadCommands(m_UnitCommandsInProcessCache);
			m_UnitCommandsInProcessCache.Fill(m_UnitCommandsInProcess);
			foreach (UnitCommandParams item in m_UnitCommandsInProcess)
			{
				BaseUnitEntity entity = item.OwnerRef.Entity;
				if (entity == null)
				{
					PFLog.Replay.Error("UnitEntity is null. ownerRef='" + item.OwnerRef.Id + "'");
					continue;
				}
				PartUnitCommands commands = entity.Commands;
				switch (item.Type)
				{
				case UnitCommandParams.CommandType.Run:
					commands.RunImmediate(item);
					break;
				case UnitCommandParams.CommandType.AddToQueue:
					commands.ForceAddToQueue(item);
					break;
				case UnitCommandParams.CommandType.AddToQueueFirst:
					commands.ForceAddToQueueFirst(item);
					break;
				default:
					throw new ArgumentOutOfRangeException($"unitCommandWrapper.type={item.Type}");
				}
			}
			m_UnitCommandsInProcess.Clear();
		}
	}

	void IControllerStart.OnStart()
	{
		DumpScheduledCommands();
	}

	void IControllerStop.OnStop()
	{
		DumpScheduledCommands();
	}

	public void CancelCurrentCommands()
	{
		m_UnitCommands.Clear();
		m_CommandQueue.CancelCurrentCommands();
	}

	public void Clear()
	{
		m_UnitCommands.Clear();
		m_UnitCommandsInProcess.Clear();
		m_CommandQueue.Reset();
		m_UnitCommandsInProcessCache.Clear();
	}

	public void DumpScheduledCommands()
	{
		foreach (UnitCommandParams unitCommand in m_UnitCommands)
		{
			Logger.Warning("Command {0} is still in queue during loading", unitCommand);
		}
		m_CommandQueue.DumpScheduledCommands();
	}
}
