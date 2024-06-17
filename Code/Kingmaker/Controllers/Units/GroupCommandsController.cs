using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Controllers.Units;

public class GroupCommandsController : IControllerTick, IController, IControllerReset
{
	[NotNull]
	private readonly List<GroupCommand> m_Commands = new List<GroupCommand>();

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		bool flag = false;
		foreach (GroupCommand command in m_Commands)
		{
			if (command.IsFinished)
			{
				flag = true;
				continue;
			}
			if (!command.IsStarted)
			{
				command.TickBeforeStart();
				if (command.CanStart)
				{
					command.Start();
				}
				continue;
			}
			command.Tick();
			if (command.ReadyToAct)
			{
				command.RunAction();
				flag = true;
			}
		}
		if (flag)
		{
			m_Commands.RemoveAll((GroupCommand c) => c.IsFinished);
		}
	}

	void IControllerReset.OnReset()
	{
		m_Commands.Clear();
	}

	public void ClearDuplicates(Type command)
	{
		try
		{
			foreach (GroupCommand item in m_Commands.ToList())
			{
				if (item.GetType() == command)
				{
					item.Interrupt();
					m_Commands.Remove(item);
				}
			}
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}

	public AreaTransitionGroupCommand RequestAreaTransition(Guid groupGuid, [NotNull] IEnumerable<EntityRef<BaseUnitEntity>> units, [NotNull] AreaTransitionPart transition)
	{
		AreaTransitionGroupCommand areaTransitionGroupCommand = m_Commands.OfType<AreaTransitionGroupCommand>().FirstOrDefault((AreaTransitionGroupCommand i) => i.GroupGuid == groupGuid);
		if (areaTransitionGroupCommand == null)
		{
			areaTransitionGroupCommand = new AreaTransitionGroupCommand(groupGuid, units, transition);
			m_Commands.Add(areaTransitionGroupCommand);
		}
		return areaTransitionGroupCommand;
	}
}
