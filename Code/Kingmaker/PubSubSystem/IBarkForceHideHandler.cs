using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBarkForceHideHandler : ISubscriber
{
	void ForceHideBarkHandle();
}
