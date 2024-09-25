using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICargoSelectHandler : ISubscriber
{
	void HandleSelectCargo(CargoSlotVM cargoSlot);
}
