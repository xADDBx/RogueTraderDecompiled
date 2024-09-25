using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.UnitLogic.Commands;

public class UnitJumpAsideDodge : UnitCommand<UnitJumpAsideDodgeParams>
{
	public override bool IsMoveUnit => true;

	public UnitJumpAsideDodge([NotNull] UnitJumpAsideDodgeParams @params)
		: base(@params)
	{
	}

	public override void Tick()
	{
		using (ProfileScope.New("OnTick"))
		{
			OnTick();
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}

	public override void OnRun()
	{
		base.OnRun();
		base.Executor.View.MovementAgent.Blocker.BlockAt(base.ForcedPath.vectorPath.Last());
	}

	protected override void OnStart()
	{
		base.OnStart();
		base.Executor.Features.DoNotProvokeAttacksOfOpportunity.Retain();
		base.Executor.View.MovementAgent.ForcePath(base.ForcedPath);
		base.Executor.CombatState.LastDiagonalCount += base.ForcedPath.DiagonalsCount();
	}

	protected override void OnTick()
	{
		base.OnTick();
		if ((bool)base.Executor.Features.CantMove)
		{
			ForceFinish(ResultType.Fail);
		}
		if (!base.Executor.View.MovementAgent.WantsToMove)
		{
			ForceFinish(ResultType.Success);
		}
		if (!base.Executor.View.MovementAgent.IsReallyMoving)
		{
			base.Executor.Position = base.ForcedPath.vectorPath.Last();
			ForceFinish(ResultType.Success);
		}
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		base.Executor.Features.DoNotProvokeAttacksOfOpportunity.Release();
	}
}
