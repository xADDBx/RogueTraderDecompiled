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
	}

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public float UnmarkAfter;

	private bool m_SkippedByPlayer;

	public override bool IsContinuous
	{
		get
		{
			if (UnmarkAfter <= 0f)
			{
				return !m_SkippedByPlayer;
			}
			return false;
		}
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
	}

	public override IAbstractUnitEntity GetControlledUnit()
	{
		if (!Unit || !Unit.TryGetValue(out var value))
		{
			return null;
		}
		return value;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		if (IsContinuous)
		{
			return false;
		}
		return player.GetCommandData<Data>(this).IsFinished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).IsFinished = time > (double)UnmarkAfter;
	}

	public override string GetCaption()
	{
		return "<b>Mark</b> " + (Unit ? Unit.GetCaption() : "???") + ((UnmarkAfter > 0f) ? (" for " + UnmarkAfter + " secs") : " indefinitely");
	}

	public override void SkipByPlayer()
	{
		m_SkippedByPlayer = true;
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
