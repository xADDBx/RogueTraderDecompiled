using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISettingsUIHandler : ISubscriber
{
	void HandleOpenSettings(bool isMainMenu = false);
}
