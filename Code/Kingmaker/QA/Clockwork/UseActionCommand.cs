using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/UseActionCommand")]
[TypeId("6c0ddb9c8878cb646ac0d997bb9c1f58")]
public class UseActionCommand : ClockworkCommand
{
	[ValidateNotNull]
	[SerializeReference]
	public GameAction action;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		Complete();
		action.RunAction();
		return null;
	}

	public override string GetCaption()
	{
		return $"{GetStatusString()}Use action {action}";
	}
}
