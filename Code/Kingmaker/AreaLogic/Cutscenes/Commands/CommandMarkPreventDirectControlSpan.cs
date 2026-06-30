using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("e46b995c49598ac458a560aaa64a3a9c")]
public class CommandMarkPreventDirectControlSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public BaseUnitEntity Unit;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!(Unit.GetValue() is BaseUnitEntity unit))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Cutscene command {this}, {Unit} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			commandData.Unit = unit;
			commandData.Unit.PreventDirectControl.Retain();
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.PreventDirectControl.Release();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return false;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "Mark " + Unit?.GetCaptionShort() + " <b>prevent direct control</b>";
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
