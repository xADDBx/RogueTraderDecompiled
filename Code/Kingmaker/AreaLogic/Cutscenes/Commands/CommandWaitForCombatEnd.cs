using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("81b7d7d16fb94c3ab5eff545982da12a")]
public class CommandWaitForCombatEnd : CommandBase
{
	private class Data
	{
		public double CurrentTime;
	}

	[SerializeField]
	[Tooltip("The command will end even in combat if it takes longer than this")]
	private float m_TimeOut = 60f;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		player.ClearCommandData(this);
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		player.GetCommandData<Data>(this).CurrentTime = time;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		Game.Instance.Player.UpdateIsInCombat();
		if (player.GetCommandData<Data>(this).CurrentTime > (double)m_TimeOut)
		{
			PFLog.Default.ErrorWithReport("Command " + name + " in " + player.Cutscene.name + " is taking too long, skipping");
			return true;
		}
		return !Game.Instance.Player.IsInCombat;
	}

	public override string GetCaption()
	{
		return "Wait for combat to end";
	}
}
