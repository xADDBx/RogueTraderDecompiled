using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IEndSpaceCombatHandler : ISubscriber
{
	void HandleEndSpaceCombat();
}
