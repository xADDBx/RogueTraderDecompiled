using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("b1e26f75fdb925948b85eaf630651df8")]
public class CommandMarkHiddenSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool NoFadeOut;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.Unit?.Features.Hidden.Retain();
		if (NoFadeOut)
		{
			AbstractUnitEntity unit = commandData.Unit;
			if (((unit == null) ? null : unit.View.Or(null)?.Fader) != null)
			{
				commandData.Unit.View.Fader.Visible = false;
				commandData.Unit.View.Fader.FastForward();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).Unit?.Features.Hidden.Release();
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
		return "Mark " + Unit?.GetCaption() + " <b>hidden</b>";
	}
}
