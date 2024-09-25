using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("ce685e563b57ba14b8c03ba1ef90e435")]
public class CommandDelay : CommandBase
{
	private class Data
	{
		public float Time;

		public bool Finished;
	}

	public float Time;

	[ConditionalShow("Random")]
	public float MaxTime;

	public bool Random;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		player.GetCommandData<Data>(this).Time = (skipping ? 0.0001f : (Random ? player.Random.Range(Time, MaxTime) : Time));
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).Finished;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.Finished = time >= (double)commandData.Time;
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		player.GetCommandData<Data>(this).Finished = true;
	}

	public override string GetCaption()
	{
		if (!Random)
		{
			return $"<b>Delay</b> {Time} secs";
		}
		return $"<b>Delay</b> {Time}-{MaxTime} secs";
	}
}
