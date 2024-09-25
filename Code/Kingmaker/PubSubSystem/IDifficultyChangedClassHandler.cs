using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDifficultyChangedClassHandler : ISubscriber
{
	void HandleDifficultyChanged();
}
