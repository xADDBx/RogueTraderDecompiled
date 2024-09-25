using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("55b6ef9f90a741b988c23e05c362d556")]
public class CommandMarkPassiveSpan : CommandBase
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
		commandData.Unit?.Passive.Retain();
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Passive.Release();
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
		return "Mark " + Unit?.GetCaption() + " <b>passive</b>";
	}
}
