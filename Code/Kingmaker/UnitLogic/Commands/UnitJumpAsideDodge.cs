using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;

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
		if (base.Executor == null)
		{
			PFLog.Ability.Error("Executor for jump dodge is missing");
			ForceFinish(ResultType.Success);
		}
		FeatureCountableFlag featureCountableFlag = base.Executor.Features?.CantMove;
		if (featureCountableFlag != null && (bool)featureCountableFlag)
		{
			ForceFinish(ResultType.Fail);
		}
		UnitEntityView view = base.Executor.View;
		bool? obj;
		if ((object)view == null)
		{
			obj = null;
		}
		else
		{
			UnitMovementAgentBase movementAgent = view.MovementAgent;
			obj = (((object)movementAgent != null) ? new bool?(!movementAgent.WantsToMove) : null);
		}
		if (obj ?? true)
		{
			ForceFinish(ResultType.Success);
		}
		if (!base.Executor.View.MovementAgent.IsReallyMoving)
		{
			if (base.ForcedPath != null)
			{
				base.Executor.Position = base.ForcedPath.vectorPath.Last();
			}
			ForceFinish(ResultType.Success);
		}
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		base.Executor.Features.DoNotProvokeAttacksOfOpportunity.Release();
	}
}
