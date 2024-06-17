using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICharInfoAbilitiesChooseModeHandler : ISubscriber
{
	void HandleChooseMode(bool active);
}
