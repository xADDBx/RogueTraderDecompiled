using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/MoveCommand")]
[TypeId("0a574d5d87976144dbadf8f10142a798")]
public class MoveCommand : ClockworkCommand
{
	[ValidateNotNull]
	public Vector3 PointToMove;

	public MoveCommand()
	{
	}

	public MoveCommand(Vector3 pointToMove)
	{
		PointToMove = pointToMove;
	}

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		TaskMovePartyToPoint taskMovePartyToPoint = new TaskMovePartyToPoint(runner, PointToMove);
		taskMovePartyToPoint.SetSourceCommand(this);
		return taskMovePartyToPoint;
	}

	public override string GetCaption()
	{
		return $"{GetStatusString()}Move to point {PointToMove}";
	}
}
