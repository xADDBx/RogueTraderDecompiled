using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("bc6b97d0e8d8be94aa5102b16fe751df")]
public class CommandMarkOnElevatorSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.Unit?.Features.OnElevator.Retain();
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Features.OnElevator.Release();
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
		return "Mark " + Unit?.GetCaption() + " <b>on elevator</b>";
	}
}
