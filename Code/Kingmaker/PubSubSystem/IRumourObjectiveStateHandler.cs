using Kingmaker.Blueprints.Quests;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IRumourObjectiveStateHandler : ISubscriber
{
	void HandleRumourObjectiveActiveStateChanged(BlueprintQuestObjective objective, bool isActive);
}
