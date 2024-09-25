using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ICreditsWindowUIHandler : ISubscriber
{
	void HandleOpenCredits();
}
