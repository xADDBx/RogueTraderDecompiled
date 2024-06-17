using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapWillChangeNavigatorResourceEffectHandler : ISubscriber
{
	void HandleWillChangeNavigatorResourceEffect(bool state, int count);
}
