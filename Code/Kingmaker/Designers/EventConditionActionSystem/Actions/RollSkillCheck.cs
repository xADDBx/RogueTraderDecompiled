using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SkillCheck")]
[AllowMultipleComponents]
[TypeId("5d7ebf10f1d4a514481b8779f2f728c5")]
public class RollSkillCheck : GameAction
{
	public StatType Stat;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public int DC;

	public bool LogSuccess = true;

	public bool LogFailure;

	public bool Voice = true;

	[InfoBox("In capital and camp all rolls are made by party (by default). Set to true to prevent it")]
	public bool ForbidPartyHelpInCamp;

	public ActionList OnSuccess;

	public ActionList OnFailure;

	public override void RunAction()
	{
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(Unit.GetValue(), Stat, DC);
		if (Voice)
		{
			rulePerformSkillCheck.Voice = (LogFailure ? RulePerformSkillCheck.VoicingType.Failure : RulePerformSkillCheck.VoicingType.None) | (LogSuccess ? RulePerformSkillCheck.VoicingType.Success : RulePerformSkillCheck.VoicingType.None);
		}
		RulePerformSkillCheck result = GameHelper.TriggerSkillCheck(rulePerformSkillCheck, null, !ForbidPartyHelpInCamp);
		if ((LogFailure && !result.ResultIsSuccess) || (LogSuccess && result.ResultIsSuccess))
		{
			EventBus.RaiseEvent(delegate(IRollSkillCheckHandler h)
			{
				h.HandleUnitSkillCheckRolled(result);
			});
		}
		if (result.ResultIsSuccess)
		{
			OnSuccess.Run();
		}
		else
		{
			OnFailure.Run();
		}
	}

	public override string GetCaption()
	{
		return $"Check {Stat} DC {DC}";
	}
}
