using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Commands.Base;

public class UnitCommandHandle
{
	private static readonly List<UnitCommandHandle> All = new List<UnitCommandHandle>();

	[NotNull]
	public readonly UnitCommandParams Params;

	[CanBeNull]
	private AbstractUnitCommand m_Cmd;

	private int m_ExpirationSystemStepIndex;

	[CanBeNull]
	public AbstractUnitCommand Cmd
	{
		get
		{
			return m_Cmd;
		}
		private set
		{
			if (m_Cmd != null)
			{
				throw new InvalidOperationException();
			}
			m_Cmd = value;
			m_ExpirationSystemStepIndex = Game.Instance.RealTimeController.SystemStepIndexAfter(10.Seconds());
		}
	}

	public AbstractUnitCommand.ResultType Result => Cmd?.Result ?? AbstractUnitCommand.ResultType.None;

	[CanBeNull]
	public AbstractUnitEntity Executor => Cmd?.Executor;

	[CanBeNull]
	public TargetWrapper Target => Params.Target;

	public float TimeSinceStart => Cmd?.TimeSinceStart ?? 0f;

	private bool Unused
	{
		get
		{
			if (Result == AbstractUnitCommand.ResultType.None)
			{
				AbstractUnitCommand cmd = m_Cmd;
				if (cmd == null || !cmd.IsFinished)
				{
					AbstractUnitCommand cmd2 = m_Cmd;
					if (cmd2 == null || !cmd2.Executor.IsDisposed)
					{
						if (m_Cmd == null)
						{
							return m_ExpirationSystemStepIndex <= Game.Instance.RealTimeController.CurrentSystemStepIndex;
						}
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool IsFinished
	{
		get
		{
			if (!Unused)
			{
				AbstractUnitCommand cmd = Cmd;
				if (cmd == null || !cmd.IsFinished)
				{
					return Result != AbstractUnitCommand.ResultType.None;
				}
			}
			return true;
		}
	}

	private UnitCommandHandle([NotNull] UnitCommandParams @params, [CanBeNull] AbstractUnitCommand cmd = null)
	{
		Params = @params;
		Cmd = cmd;
		All.Add(this);
	}

	public void ForceFinishForTurnBased(AbstractUnitCommand.ResultType result)
	{
		Cmd?.ForceFinishForTurnBased(result);
	}

	public void Interrupt()
	{
		Cmd?.Interrupt();
	}

	[NotNull]
	public static UnitCommandHandle Request([NotNull] UnitCommandParams @params, [CanBeNull] AbstractUnitCommand cmd = null)
	{
		UnitCommandHandle unitCommandHandle = All.FirstOrDefault((UnitCommandHandle i) => i.Params == @params);
		if (unitCommandHandle == null)
		{
			return new UnitCommandHandle(@params, cmd);
		}
		if (cmd != null && unitCommandHandle.Cmd != null && unitCommandHandle.Cmd != cmd)
		{
			throw new Exception("Cmd is already set");
		}
		unitCommandHandle.Cmd = cmd;
		return unitCommandHandle;
	}

	public static UnitCommandHandle Request([NotNull] AbstractUnitCommand cmd)
	{
		return All.FirstOrDefault((UnitCommandHandle i) => i.Cmd == cmd) ?? Request(cmd.Params, cmd);
	}

	public static void Cleanup()
	{
		All.RemoveAll((UnitCommandHandle i) => i.Unused);
	}
}
