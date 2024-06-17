using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public interface IShipComponentItemHandler : ISubscriber
{
	void HandleChangeItem(ShipComponentSlotVM slot);
}
