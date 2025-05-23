using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IChangeAppearanceCloseHandler : ISubscriber
{
	void HandleCloseChangeAppearance();
}
