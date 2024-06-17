using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.QA.Validation;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/AnswerCommand")]
[TypeId("a01bcf817ff157c4582fd236bb3a4333")]
public class AnswerCommand : ClockworkCommand
{
	[ValidateNotNull]
	public BlueprintAnswerReference Answer;

	public AnswerCommand()
	{
	}

	public AnswerCommand(BlueprintAnswer answer)
	{
		Answer = answer.ToReference<BlueprintAnswerReference>();
	}

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		TaskAnswerDialog taskAnswerDialog = new TaskAnswerDialog(runner, Answer);
		taskAnswerDialog.SetSourceCommand(this);
		return taskAnswerDialog;
	}

	public override string GetCaption()
	{
		string arg = Answer.Get().DisplayText.Substring(0, Math.Min(Answer.Get().DisplayText.Length, 30));
		return $"{GetStatusString()}Select answer <{arg}> {Answer}";
	}
}
