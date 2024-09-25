using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IExitSpaceCombatHandler : ISubscriber
{
	void HandleExitSpaceCombat();
}
