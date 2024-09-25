using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISelectingWindowFocusHandler : ISubscriber
{
	void Focus(ShipComponentItemSlotVM val);
}
