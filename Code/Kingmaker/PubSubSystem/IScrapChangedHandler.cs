using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IScrapChangedHandler : ISubscriber
{
	void HandleScrapGained(int scrap);

	void HandleScrapSpend(int scrap);
}
