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

	[SerializeField]
	[Tooltip("Timeout for triggering animations after the end of a battle, if the battle was ended forcibly, without a timeout.")]
	private float m_IsIgnoreLeaveTimerTimeOut = 2.5f;

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
		Data commandData = player.GetCommandData<Data>(this);
		if (commandData.CurrentTime > (double)m_TimeOut)
		{
			PFLog.Default.ErrorWithReport("Command " + name + " in " + player.Cutscene.name + " is taking too long, skipping");
			return true;
		}
		if (!Game.Instance.Player.IsInCombat && Game.Instance.Player.LastCombatLeaveIgnoreLeaveTimer && commandData.CurrentTime < (double)m_IsIgnoreLeaveTimerTimeOut)
		{
			return false;
		}
		return !Game.Instance.Player.IsInCombat;
	}

	public override string GetCaption()
	{
		return "Wait for combat to end";
	}
}
