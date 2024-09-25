using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IInsertItemHandler : ISubscriber
{
	void HandleInsertItem(ItemSlot slot);
}
