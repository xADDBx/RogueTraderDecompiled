using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;

namespace Kingmaker.PubSubSystem;

public interface IFullScreenUIHandler : ISubscriber
{
	void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType);
}
