using System;
using System.Collections;
using Kingmaker.AI;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.QA.Arbiter;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class TaskAutoCombat : ClockworkRunnerTask
{
	private MechanicEntity m_CurrentUnit;

	private float m_UnitTurnTimeout;

	public TaskAutoCombat(ClockworkRunner runner)
		: base(runner)
	{
		m_UnitTurnTimeout = BlueprintArbiterRoot.Instance.UnitCombatTurnTimeout;
		TimeLeft = BlueprintArbiterRoot.Instance.CombatTimeout;
	}

	protected override IEnumerator Routine()
	{
		AiBrainController.AutoCombat = true;
		while (GameHelper.GetPlayerCharacter().IsInCombat)
		{
			if (Game.Instance.TurnController.IsPreparationTurn)
			{
				Game.Instance.TurnController.RequestEndPreparationTurn();
			}
			if (IsTurnTimeOut())
			{
				AiBrainController.AutoCombat = false;
				throw new Exception("Combat turn timeout");
			}
			yield return null;
		}
		AiBrainController.AutoCombat = false;
		yield return new TaskHeal(null);
	}

	private bool IsTurnTimeOut()
	{
		if (Game.Instance.TurnController.CurrentUnit == m_CurrentUnit)
		{
			m_UnitTurnTimeout -= Time.unscaledDeltaTime;
			if (m_UnitTurnTimeout < 0f)
			{
				return true;
			}
			return false;
		}
		m_CurrentUnit = Game.Instance.TurnController.CurrentUnit;
		m_UnitTurnTimeout = BlueprintArbiterRoot.Instance.UnitCombatTurnTimeout;
		return false;
	}
}
