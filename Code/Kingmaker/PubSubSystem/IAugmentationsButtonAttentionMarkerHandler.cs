using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentationsButtonAttentionMarkerHandler : ISubscriber
{
	void HandleAttentionMarker(bool state);
}
