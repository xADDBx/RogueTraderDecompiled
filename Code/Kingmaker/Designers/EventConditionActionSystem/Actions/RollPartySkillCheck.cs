using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SkillCheck Party")]
[AllowMultipleComponents]
[TypeId("49e3105a0ea5eb9499b86c72c715c140")]
public class RollPartySkillCheck : GameAction
{
	public StatType Stat;

	public int DC;

	public bool LogSuccess = true;

	public bool LogFailure;

	public ActionList OnSuccess;

	public ActionList OnFailure;

	protected override void RunAction()
	{
		RulePerformPartySkillCheck check = new RulePerformPartySkillCheck(Stat, DC)
		{
			DisableGameLog = true
		};
		Rulebook.Trigger(check);
		if ((LogFailure && !check.Success) || (LogSuccess && check.Success))
		{
			EventBus.RaiseEvent(delegate(IRollSkillCheckHandler h)
			{
				h.HandlePartySkillCheckRolled(check);
			});
		}
		if (check.Success)
		{
			using (ContextData<CheckPassedData>.Request().Setup(check))
			{
				OnSuccess.Run();
				return;
			}
		}
		OnFailure.Run();
	}

	public override string GetCaption()
	{
		return $"Check party {Stat} DC {DC}";
	}
}
