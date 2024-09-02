using JetBrains.Annotations;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public class UnitInterruptTurn : UnitCommand<UnitInterruptTurnParams>
{
	public override bool IsMoveUnit => false;

	public UnitInterruptTurn([NotNull] UnitInterruptTurnParams @params)
		: base(@params)
	{
	}

	protected override ResultType OnAction()
	{
		Game.Instance.TurnController.InterruptCurrentTurnByCommand(base.Params.EntityToGetTheTurn, base.Params.InterruptionSource, base.Params.InterruptionData);
		return ResultType.Success;
	}
}
