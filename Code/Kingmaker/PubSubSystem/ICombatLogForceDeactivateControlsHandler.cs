using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatLogForceDeactivateControlsHandler : ISubscriber
{
	void HandleCombatLogForceDeactivateControls();
}
