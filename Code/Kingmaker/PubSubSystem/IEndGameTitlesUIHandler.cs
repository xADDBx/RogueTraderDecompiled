using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEndGameTitlesUIHandler : ISubscriber
{
	void HandleShowEndGameTitles(bool returnToMainMenu);
}
