using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Commands.Base;

public abstract class GroupCommand
{
	[NotNull]
	protected readonly BaseUnitEntity[] Units;

	[NotNull]
	protected readonly List<BaseUnitEntity> ActedUnits = new List<BaseUnitEntity>();

	public readonly Guid GroupGuid;

	[NotNull]
	public readonly TargetWrapper Target;

	public bool IsStarted { get; private set; }

	public float Time { get; private set; }

	public AbstractUnitCommand.ResultType Result { get; private set; }

	public bool IsFinished => Result != AbstractUnitCommand.ResultType.None;

	public virtual bool ReadyToAct => true;

	public virtual bool CanStart => ActedUnits.Count >= Units.Length;

	protected GroupCommand(Guid groupGuid, [NotNull] IEnumerable<EntityRef<BaseUnitEntity>> units, [NotNull] TargetWrapper target)
	{
		GroupGuid = groupGuid;
		Units = units.Select((Func<EntityRef<BaseUnitEntity>, BaseUnitEntity>)((EntityRef<BaseUnitEntity> elem) => elem)).ToArray();
		Target = target;
	}

	public void Interrupt()
	{
		if (!IsFinished)
		{
			Result = AbstractUnitCommand.ResultType.Interrupt;
			OnInterrupt();
			Cleanup();
		}
	}

	public void Start()
	{
		if (!IsStarted)
		{
			IsStarted = true;
			OnStart();
		}
	}

	public void TickBeforeStart()
	{
		OnTickBeforeStart();
	}

	public void Tick()
	{
		Time += Game.Instance.TimeController.DeltaTime;
		OnTick();
	}

	public void RunAction()
	{
		Result = OnAction();
		Cleanup();
	}

	public void OnUnitAction(BaseUnitEntity unit)
	{
		if (!IsFinished)
		{
			ActedUnits.Add(unit);
		}
	}

	protected virtual void Cleanup()
	{
		BaseUnitEntity[] units = Units;
		for (int i = 0; i < units.Length; i++)
		{
			units[i].Commands.InterruptGroupCommand();
		}
	}

	protected virtual void OnInterrupt()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnTickBeforeStart()
	{
	}

	protected virtual void OnTick()
	{
	}

	protected abstract AbstractUnitCommand.ResultType OnAction();
}
