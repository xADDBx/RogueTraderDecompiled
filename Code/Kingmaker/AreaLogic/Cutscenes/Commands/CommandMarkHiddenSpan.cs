using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
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

		public bool ShouldUnhidePet;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool NoFadeOut;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.ShouldUnhidePet = false;
		doRun(commandData.Unit);
		if (commandData.Unit is BaseUnitEntity { Pet: not null } baseUnitEntity && !baseUnitEntity.Pet.Features.Hidden.Value)
		{
			commandData.ShouldUnhidePet = true;
			doRun(baseUnitEntity.Pet);
		}
		void doRun(AbstractUnitEntity abstractUnitEntity)
		{
			abstractUnitEntity?.Features.Hidden.Retain();
			if (NoFadeOut && ((abstractUnitEntity == null) ? null : abstractUnitEntity.View.Or(null)?.Fader) != null)
			{
				abstractUnitEntity.View.Fader.Visible = false;
				abstractUnitEntity.View.Fader.FastForward();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit?.Features.Hidden.Release();
		if (commandData.ShouldUnhidePet && commandData.Unit is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.Pet?.Features.Hidden.Release();
		}
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
		return "Mark " + Unit?.GetCaptionShort() + " <b>hidden</b>";
	}
}
