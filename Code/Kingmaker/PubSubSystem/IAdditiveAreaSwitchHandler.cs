using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAdditiveAreaSwitchHandler : ISubscriber
{
	void OnAdditiveAreaBeginDeactivated();

	void OnAdditiveAreaDidActivated();
}
