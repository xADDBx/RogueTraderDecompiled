using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAreaHandler : ISubscriber
{
	void OnAreaBeginUnloading();

	void OnAreaDidLoad();
}
