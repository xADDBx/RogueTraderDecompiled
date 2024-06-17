using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombatLogBarkHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnShowBark(string text);
}
