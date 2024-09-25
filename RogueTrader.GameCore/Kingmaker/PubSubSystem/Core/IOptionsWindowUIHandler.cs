using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IOptionsWindowUIHandler : ISubscriber
{
	void HandleItemChanged(string key);
}
