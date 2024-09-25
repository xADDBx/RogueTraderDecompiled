using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IItemChargesHandler : ISubscriber
{
	void HandleItemChargeSpent(ItemEntity item);
}
