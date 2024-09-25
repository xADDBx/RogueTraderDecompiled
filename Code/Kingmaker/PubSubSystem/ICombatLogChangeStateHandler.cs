using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatLogChangeStateHandler : ISubscriber
{
	void CombatLogChangeState();
}
