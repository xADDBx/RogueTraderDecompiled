using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEncyclopediaGlossaryModeHandler : ISubscriber
{
	void HandleGlossaryMode(bool state);
}
