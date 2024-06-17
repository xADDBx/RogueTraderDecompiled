using JetBrains.Annotations;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public class UnitTeleport : UnitCommand<UnitTeleportParams>
{
	public Vector3 Position => base.Params.Position;

	public override bool IsMoveUnit => true;

	public UnitTeleport([NotNull] UnitTeleportParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		base.Executor.Position = Position;
		base.Executor.SnapToGrid();
		return ResultType.Success;
	}
}
