using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IContinueLoadingHandler : ISubscriber
{
	void HandleContinueLoading();
}
