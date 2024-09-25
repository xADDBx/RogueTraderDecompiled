using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInfoWindowHandler : ISubscriber
{
	void HandleCloseTooltipInfoWindow();
}
