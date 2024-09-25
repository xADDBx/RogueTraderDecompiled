using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IMoveItemHandler : ISubscriber
{
	void HandleMoveItem(bool isEquip);
}
