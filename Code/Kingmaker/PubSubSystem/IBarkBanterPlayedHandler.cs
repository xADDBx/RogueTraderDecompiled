using Kingmaker.BarkBanters;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBarkBanterPlayedHandler : ISubscriber
{
	void HandleBarkBanter(BlueprintBarkBanter barkBanter);
}
