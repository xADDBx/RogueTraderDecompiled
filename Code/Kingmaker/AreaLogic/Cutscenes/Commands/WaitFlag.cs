using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("09e0c54cbcb526c44bf099f8ea03d42d")]
public class WaitFlag : CommandBase
{
	[SerializeField]
	[FormerlySerializedAs("Flag")]
	private BlueprintUnlockableFlagReference m_Flag;

	public BlueprintUnlockableFlag Flag => m_Flag?.Get();

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return Flag.IsUnlocked;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
