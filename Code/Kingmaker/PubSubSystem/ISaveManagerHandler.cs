using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISaveManagerHandler : ISubscriber
{
	void HandleBeforeMadeScreenshot();

	void HandleAfterMadeScreenshot();
}
