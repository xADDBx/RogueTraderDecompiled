using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISpaceCombatRewardUIHandler : ISubscriber
{
	void HandleSpaceCombatReward(List<BlueprintItemReference> items, List<int> itemCounts, List<BlueprintCargoReference> cargoes, int scrap);
}
