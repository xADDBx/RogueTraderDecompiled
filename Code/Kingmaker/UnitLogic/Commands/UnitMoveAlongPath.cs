using JetBrains.Annotations;
using Kingmaker.UnitLogic.Commands.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitMoveAlongPath : AbstractUnitCommand<UnitMoveAlongPathParams>
{
	public override bool IsMoveUnit => true;

	public UnitMoveAlongPath([NotNull] UnitMoveAlongPathParams @params)
		: base(@params)
	{
	}

	protected override Vector3 GetTargetPoint()
	{
		return base.ForcedPath.vectorPath[0];
	}

	public override void TurnToTarget()
	{
	}

	protected override ResultType OnAction()
	{
		if (base.Executor.View.MovementAgent.IsReallyMoving)
		{
			return ResultType.None;
		}
		base.Executor.View.StopMoving();
		base.Executor.View.MovementAgent.AvoidanceDisabled = true;
		base.Executor.View.MovementAgent.ForcePath(base.ForcedPath, disableApproachRadius: true);
		return ResultType.Success;
	}
}
