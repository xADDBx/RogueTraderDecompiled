using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IKeybindChanged : ISubscriber
{
	void OnKeybindChanged();
}
