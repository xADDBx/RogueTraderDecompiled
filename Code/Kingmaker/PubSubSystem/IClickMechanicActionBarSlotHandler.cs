using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;

namespace Kingmaker.PubSubSystem;

public interface IClickMechanicActionBarSlotHandler : ISubscriber
{
	void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability);
}
