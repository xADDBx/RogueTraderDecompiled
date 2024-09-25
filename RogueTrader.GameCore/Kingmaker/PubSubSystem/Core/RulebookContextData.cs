using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookContextData : ContextData<RulebookContextData>
{
	public IRulebookEvent Rule { get; private set; }

	public RulebookContextData Setup(IRulebookEvent rule)
	{
		Rule = rule;
		return this;
	}

	protected override void Reset()
	{
		Rule = null;
	}
}
