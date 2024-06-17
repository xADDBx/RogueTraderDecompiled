using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface ILootHandler : ISubscriber
{
	void HandleChangeLoot(ItemSlotVM slot);
}
