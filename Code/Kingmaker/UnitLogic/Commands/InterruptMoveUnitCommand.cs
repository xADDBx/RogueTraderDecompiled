using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public sealed class InterruptMoveUnitCommand : UnitCommand<InterruptMoveUnitCommandParams>
{
	public override bool IsMoveUnit => false;

	public InterruptMoveUnitCommand(InterruptMoveUnitCommandParams @params)
		: base(@params)
	{
	}

	public override void OnRun()
	{
		base.OnRun();
		base.Executor.GetOptional<PartUnitCommands>()?.ForceInterruptMove();
	}

	protected override ResultType OnAction()
	{
		return ResultType.Success;
	}
}
