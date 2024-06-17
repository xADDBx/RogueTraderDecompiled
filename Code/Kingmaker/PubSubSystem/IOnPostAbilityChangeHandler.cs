using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IOnPostAbilityChangeHandler : ISubscriber
{
	void HandlePostAbilityChange(int postType);
}
