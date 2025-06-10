using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUpdateShieldPatternBeforeAreaLoadHandler : ISubscriber
{
	void HandleUpdateShieldPatternBeforeAreaLoad();
}
