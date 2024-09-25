using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public abstract class UnitGroupCommand : UnitCommand<UnitGroupCommandParams>
{
	private GroupCommand m_GroupCmd;

	[NotNull]
	public GroupCommand GroupCommand => m_GroupCmd ?? (m_GroupCmd = RequestGroupCommand(base.Params.GroupGuid, base.Params.Units));

	protected UnitGroupCommand([NotNull] UnitGroupCommandParams @params)
		: base(@params)
	{
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		if (base.Result != ResultType.Success)
		{
			GroupCommand.Interrupt();
		}
		else
		{
			GroupCommand.OnUnitAction(base.Executor);
		}
	}

	protected abstract GroupCommand RequestGroupCommand(Guid groupGuid, [NotNull] IEnumerable<EntityRef<BaseUnitEntity>> units);
}
public abstract class UnitGroupCommand<T> : UnitGroupCommand where T : UnitGroupCommandParams
{
	public new T Params => (T)base.Params;

	protected UnitGroupCommand([NotNull] T @params)
		: base(@params)
	{
	}
}
