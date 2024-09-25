using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IFocusHandler : ISubscriber
{
	void OnApplicationFocusChanged(bool isFocused);
}
