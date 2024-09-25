using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Warhammer.SpaceCombat;

public interface IStarshipComponentHandler : ISubscriber
{
	void HandleComponentBlocked(ItemSlot component);
}
