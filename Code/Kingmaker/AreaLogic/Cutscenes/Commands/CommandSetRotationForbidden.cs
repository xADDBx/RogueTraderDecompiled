using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("d4321566366803c40bb6aa97d51b92f6")]
public class CommandSetRotationForbidden : CommandBase
{
	private class Data
	{
		public AbstractUnitEntity Unit;
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.Unit?.Features.RotationForbidden.Retain();
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Features.RotationForbidden.Release();
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
		return "Set <b>Rotation Forbidden</b> for " + Unit?.GetCaptionShort();
	}
}
