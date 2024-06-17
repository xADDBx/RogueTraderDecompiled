using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEncyclopediaNodeViewedHandler : ISubscriber
{
	void HandleEncyclopediaNodeViewed(BlueprintEncyclopediaNode node);
}
