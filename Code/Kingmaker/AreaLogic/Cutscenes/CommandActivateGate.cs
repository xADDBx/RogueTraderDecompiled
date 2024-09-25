using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("bd6bb1af71d936e40b120344853cd3fc")]
public class CommandActivateGate : CommandBase
{
	public CommandSignalData SignalData = new CommandSignalData
	{
		Name = "Gate"
	};

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		player.ReactivateGate(SignalData.Gate);
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}

	public override string GetCaption()
	{
		return "<b>Activate</b> " + SignalData?.Gate.NameSafe();
	}

	public override CommandSignalData[] GetExtraSignals()
	{
		return new CommandSignalData[1] { SignalData };
	}
}
