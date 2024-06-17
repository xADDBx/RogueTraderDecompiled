using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapSetAllSystemsInformationWindowStateHandler : ISubscriber
{
	void HandleSetAllSystemsInformationWindowState(bool state);
}
