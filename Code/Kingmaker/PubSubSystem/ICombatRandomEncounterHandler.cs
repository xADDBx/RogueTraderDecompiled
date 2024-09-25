using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatRandomEncounterHandler : ISubscriber
{
	void HandleCombatRandomEncounterStart();

	void HandleCombatRandomEncounterFinish();
}
