using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("9d59dacab16c67d47ab0760668284391")]
public class CommandMarkUnitControlled : CommandBase
{
	private class Data
	{
		public bool IsFinished;

		public bool SkippedByPlayer;
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float UnmarkAfter;

	public override bool IsContinuous => UnmarkAfter <= 0f;

	public override bool TrySkip(CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).SkippedByPlayer = true;
		return true;
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (Unit == null || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override bool TryPrepareForStop(CutscenePlayerData player)
	{
		if ((!player.GetCommandData<Data>(this).SkippedByPlayer && IsContinuous) || !IsFinished(player))
		{
			return false;
		}
		return StopPlaySignalIsReady(player);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (!commandData.SkippedByPlayer && IsContinuous)
		{
			return false;
		}
		return commandData.IsFinished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).IsFinished = time > (double)UnmarkAfter;
	}

	public override string GetCaption()
	{
		return "<b>Mark</b> " + (Unit ? Unit.GetCaption() : "???") + ((UnmarkAfter > 0f) ? (" for " + UnmarkAfter + " secs") : " indefinitely");
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
