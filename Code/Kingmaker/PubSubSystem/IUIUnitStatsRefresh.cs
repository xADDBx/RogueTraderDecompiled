using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUIUnitStatsRefresh : ISubscriber
{
	void Refresh();
}
