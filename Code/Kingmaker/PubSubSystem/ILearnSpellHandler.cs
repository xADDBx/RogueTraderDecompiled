using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ILearnSpellHandler : ISubscriber
{
	void HandleLearnSpell();
}
