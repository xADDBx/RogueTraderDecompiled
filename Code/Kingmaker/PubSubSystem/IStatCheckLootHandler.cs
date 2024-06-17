using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IStatCheckLootHandler : ISubscriber
{
	void HandleStatCheckLootStartCheck(ICheckForLoot checkForLoot);

	void HandleStatCheckLootChecked();
}
