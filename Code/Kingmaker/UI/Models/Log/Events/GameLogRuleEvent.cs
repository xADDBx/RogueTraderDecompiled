using Kingmaker.RuleSystem;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

namespace Kingmaker.UI.Models.Log.Events;

public abstract class GameLogRuleEvent : GameLogEvent
{
	public readonly RulebookEvent Rule;

	public override bool IsEnabled
	{
		get
		{
			if (base.IsEnabled)
			{
				return !Rule.IsGameLogDisabled;
			}
			return false;
		}
	}

	protected GameLogRuleEvent(RulebookEvent rule)
	{
		Rule = rule;
	}
}
public class GameLogRuleEvent<TRule> : GameLogRuleEvent where TRule : RulebookEvent
{
	public new TRule Rule => (TRule)base.Rule;

	public GameLogRuleEvent(TRule rule)
		: base(rule)
	{
	}

	public override void Invoke(LogThreadBase logThread)
	{
		(logThread as IGameLogRuleHandler<TRule>)?.HandleEvent(Rule);
	}
}
public abstract class GameLogRuleEvent<TRule, TSelf> : GameLogRuleEvent<TRule> where TRule : RulebookEvent where TSelf : GameLogRuleEvent<TRule, TSelf>
{
	protected GameLogRuleEvent(TRule rule)
		: base(rule)
	{
	}

	public sealed override void Invoke(LogThreadBase logThread)
	{
		(logThread as IGameLogRuleHandler<TRule>)?.HandleEvent(base.Rule);
		(logThread as IGameLogEventHandler<TSelf>)?.HandleEvent((TSelf)this);
	}
}
