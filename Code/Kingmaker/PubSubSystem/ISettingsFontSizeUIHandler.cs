using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISettingsFontSizeUIHandler : ISubscriber
{
	void HandleChangeFontSizeSettings(float size);
}
