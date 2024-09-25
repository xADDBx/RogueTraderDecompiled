using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEtudesUpdateHandler : ISubscriber
{
	void OnEtudesUpdate();
}
