using Kingmaker.RuleSystem;

namespace Kingmaker.UI.Models.Log.Events;

public interface IGameLogRuleHandler<in T> where T : RulebookEvent
{
	void HandleEvent(T evt);
}
