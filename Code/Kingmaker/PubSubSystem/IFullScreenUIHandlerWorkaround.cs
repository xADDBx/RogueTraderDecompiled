using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;

namespace Kingmaker.PubSubSystem;

public interface IFullScreenUIHandlerWorkaround : ISubscriber
{
	void HandleFullScreenUiChangedWorkaround(bool state, FullScreenUIType fullScreenUIType);
}
