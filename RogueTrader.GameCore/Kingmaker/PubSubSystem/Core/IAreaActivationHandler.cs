using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAreaActivationHandler : ISubscriber
{
	void OnAreaActivated();
}
