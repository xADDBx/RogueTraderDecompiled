using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGainFactionReputationHandler : ISubscriber
{
	void HandleGainFactionReputation(FactionType factionType, int count);
}
