using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.Controllers.Units;

internal class VirtualMoveCommand
{
	[NotNull]
	public readonly UnitCommandParams CmdParams;

	public readonly UnitReference Unit;

	[CanBeNull]
	public UnitCommandHandle CmdHandle { get; private set; }

	public VirtualMoveCommand([NotNull] UnitCommandParams @params, [NotNull] BaseUnitEntity unit)
	{
		if (@params == null)
		{
			throw new ArgumentNullException("params");
		}
		if (unit == null)
		{
			throw new ArgumentNullException("unit");
		}
		CmdParams = @params;
		Unit = unit.FromBaseUnitEntity();
	}

	public void RunCommand()
	{
		CmdHandle = Unit.Entity.ToBaseUnitEntity().Commands.Run(CmdParams);
	}
}
