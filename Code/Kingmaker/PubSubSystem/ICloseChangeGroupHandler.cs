using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICloseChangeGroupHandler : ISubscriber
{
	void HandleCloseChangeGroup();
}
