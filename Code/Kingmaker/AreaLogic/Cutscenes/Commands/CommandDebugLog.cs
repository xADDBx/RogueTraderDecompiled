using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("751a27bb000496849ac5541c5d934abf")]
public class CommandDebugLog : CommandBase
{
	public string Text;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		PFLog.Default.Log("Command: " + Text);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
