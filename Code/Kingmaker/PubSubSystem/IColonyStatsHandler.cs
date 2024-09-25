using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonyStatsHandler : ISubscriber
{
	void HandleColonyStatsChanged();
}
