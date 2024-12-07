using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("acf6eb0f9cce4f05a8b81368de1c5f38")]
public class CommandAttachBuffSpan : CommandBase
{
	private class Data
	{
		[CanBeNull]
		public AbstractUnitEntity Unit;

		public Buff Buff;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public BlueprintBuffReference Buff;

	public override bool IsContinuous => true;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Unit = Unit.GetValue();
		commandData.Buff = commandData.Unit?.Buffs.Add(Buff.Get(), commandData.Unit);
		commandData.Buff?.AddSource(player);
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.Buff != null)
		{
			commandData.Unit?.Buffs.Remove(commandData.Buff);
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
		return "<b>Buff</b> " + Unit?.GetCaptionShort() + " <b>with</b> " + Buff?.Get().NameSafe();
	}
}
