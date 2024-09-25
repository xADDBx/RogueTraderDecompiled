using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitMoveContinuously : UnitCommand<UnitMoveContinuouslyParams>
{
	public override bool IsMoveUnit => true;

	public override bool DontWaitForHands => true;

	public UnitMoveContinuously([NotNull] UnitMoveContinuouslyParams @params)
		: base(@params)
	{
	}

	protected override void OnInit(AbstractUnitEntity executor)
	{
		base.OnInit(executor);
		if (!(base.Executor?.View == null))
		{
			UnitMovementAgentContinuous unitMovementAgentContinuous = base.Executor.View.gameObject.GetComponent<UnitMovementAgentContinuous>();
			if (unitMovementAgentContinuous == null)
			{
				unitMovementAgentContinuous = base.Executor.View.gameObject.AddComponent<UnitMovementAgentContinuous>();
				base.Executor.View.AgentOverride = unitMovementAgentContinuous;
				base.Executor.View.AgentOverride.Init(base.Executor.View.gameObject);
			}
			else
			{
				base.Executor.View.AgentOverride = unitMovementAgentContinuous;
			}
			unitMovementAgentContinuous.DirectionFromController = base.Params.Direction;
			unitMovementAgentContinuous.DirectionFromControllerMagnitude = base.Params.Multiplier;
		}
	}

	protected override void OnTick()
	{
		UnitMovementAgentContinuous unitMovementAgentContinuous = base.Executor?.View?.AgentOverride as UnitMovementAgentContinuous;
		if (unitMovementAgentContinuous == null)
		{
			OnEnded();
			return;
		}
		if (base.Target != null && (base.Executor.Position - base.Target.Point).sqrMagnitude < 0.01f)
		{
			OnEnded();
			return;
		}
		unitMovementAgentContinuous.DirectionFromController = base.Params.Direction;
		unitMovementAgentContinuous.DirectionFromControllerMagnitude = base.Params.Multiplier;
		UpdateMovementType(base.Params.Multiplier);
		if (!unitMovementAgentContinuous.WantsToMove && !unitMovementAgentContinuous.IsTraverseInProgress)
		{
			OnEnded();
		}
	}

	private void UpdateMovementType(float multiplier)
	{
		UnitMoveContinuouslyParams @params = base.Params;
		WalkSpeedType movementType = ((!(multiplier > 0.95f)) ? ((!(multiplier > 0.7f)) ? WalkSpeedType.Walk : WalkSpeedType.Run) : WalkSpeedType.Sprint);
		@params.MovementType = movementType;
	}

	protected override void OnEnded()
	{
		base.OnEnded();
		if (base.Executor?.View?.AgentOverride != null)
		{
			base.Executor.View.AgentOverride.Blocker.Unblock();
			base.Executor.View.AgentOverride = null;
		}
	}

	protected override ResultType OnAction()
	{
		return ResultType.None;
	}
}
