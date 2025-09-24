using System;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("de19bd4eb40decb419caec8194765ed0")]
[KDB("Performs a skill check. DOES NOT take DC increase from difficulty into account!")]
public class ContextActionSkillCheck : ContextAction
{
	[Serializable]
	private struct ConditionalDCIncrease
	{
		public ConditionsChecker Condition;

		public ContextValue Value;
	}

	public StatType Stat;

	[SerializeField]
	private ConditionalDCIncrease[] m_ConditionalDCIncrease = new ConditionalDCIncrease[0];

	public bool UseCustomDC;

	[ShowIf("UseCustomDC")]
	public ContextValue CustomDC;

	public bool CalculateDCDifference;

	public ActionList Success;

	[HideIf("CalculateDCDifference")]
	public ActionList Failure;

	[ShowIf("CalculateDCDifference")]
	public ActionList FailureDiffMoreOrEqual5Less10;

	[ShowIf("CalculateDCDifference")]
	public ActionList FailureDiffMoreOrEqual10;

	protected override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Target unit is missing");
			return;
		}
		int num = 0;
		if (m_ConditionalDCIncrease != null)
		{
			ConditionalDCIncrease[] conditionalDCIncrease = m_ConditionalDCIncrease;
			for (int i = 0; i < conditionalDCIncrease.Length; i++)
			{
				ConditionalDCIncrease conditionalDCIncrease2 = conditionalDCIncrease[i];
				if (conditionalDCIncrease2.Condition.Check())
				{
					num += conditionalDCIncrease2.Value.Calculate(base.Context);
				}
			}
		}
		int num2 = (UseCustomDC ? CustomDC.Calculate(base.Context) : 0);
		RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(base.Target.Entity, Stat, num2 + num, ignoreDCIncreaseFromDifficulty: true)
		{
			ShowAnyway = true
		}, base.Context);
		if (CalculateDCDifference)
		{
			if (rulePerformSkillCheck.ResultIsSuccess)
			{
				Success.Run();
			}
			else if (num2 - rulePerformSkillCheck.RollResult >= 5 && num2 - rulePerformSkillCheck.RollResult < 10)
			{
				FailureDiffMoreOrEqual5Less10.Run();
			}
			else if (num2 - rulePerformSkillCheck.RollResult >= 10)
			{
				FailureDiffMoreOrEqual10.Run();
			}
		}
		else if (rulePerformSkillCheck.ResultIsSuccess)
		{
			Success.Run();
		}
		else
		{
			Failure.Run();
		}
		if (rulePerformSkillCheck.ResultIsSuccess)
		{
			GameHelper.GainExperienceForSkillCheck(ExperienceHelper.GetCheckExp(num2, Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0));
		}
	}

	public override string GetCaption()
	{
		return string.Format("Skill check {0} {1}", Stat, UseCustomDC ? $"(DC: {CustomDC})" : "");
	}
}
