using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;

namespace Kingmaker.PubSubSystem;

public interface IModalWindowUIHandler : ISubscriber
{
	void HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType);
}
