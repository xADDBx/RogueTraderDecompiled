using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAugmentOverchargeSlotDragHandler : ISubscriber
{
	void HandleOverchargeSlotDragStart();

	void HandleOverchargeSlotDragEnd();
}
