using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IDropItemHandler : ISubscriber
{
	void HandleDropItem(ItemEntity item, bool isSplit);
}
