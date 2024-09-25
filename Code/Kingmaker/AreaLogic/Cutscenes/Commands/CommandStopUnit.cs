using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("bf083819ef1b6fc4f8a1dcc5106710d8")]
public class CommandStopUnit : CommandBase
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Unit.GetValue()?.Commands.InterruptAllInterruptible();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		if (Unit == null)
		{
			return "<b>Stop</b> none";
		}
		return "<b>Stop</b> " + Unit.GetCaption();
	}

	public override string GetWarning()
	{
		if ((bool)Unit && Unit.CanEvaluate())
		{
			return null;
		}
		return "No unit";
	}
}
