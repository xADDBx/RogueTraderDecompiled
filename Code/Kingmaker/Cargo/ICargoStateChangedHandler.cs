using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Cargo;

public interface ICargoStateChangedHandler : ISubscriber
{
	void HandleCreateNewCargo(CargoEntity entity);

	void HandleRemoveCargo(CargoEntity entity, bool fromMassSell);

	void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex);

	void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from);
}
