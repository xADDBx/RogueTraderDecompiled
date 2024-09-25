using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IContextMenuHandler : ISubscriber
{
	void HandleContextMenuRequest(ContextMenuCollection collection);
}
