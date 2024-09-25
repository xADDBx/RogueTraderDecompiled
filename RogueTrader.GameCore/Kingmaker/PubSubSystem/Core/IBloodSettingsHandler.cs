using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IBloodSettingsHandler : ISubscriber
{
	void HandleBloodSettingChanged();
}
