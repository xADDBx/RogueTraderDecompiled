using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models.UnitSettings;

namespace Kingmaker.PubSubSystem;

public interface IHoverActionBarSlotHandler : ISubscriber
{
	void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability);

	void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability);

	void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability);

	void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability);
}
