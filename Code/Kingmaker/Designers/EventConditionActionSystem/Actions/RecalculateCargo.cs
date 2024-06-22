using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("67ff1fc51e824dbbb6a3bbf44463bb1c")]
public class RecalculateCargo : GameAction
{
	public override string GetCaption()
	{
		return "Recalculate cargo after blueprint changes";
	}

	protected override void RunAction()
	{
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			CargoState cargoState = Game.Instance.Player.CargoState;
			List<CargoEntity> list = cargoState.CargoEntities.ToList();
			foreach (CargoEntity item in list)
			{
				cargoState.Remove(item);
			}
			foreach (CargoEntity item2 in list)
			{
				foreach (ItemEntity item3 in item2.Inventory.Items)
				{
					item2.Inventory.Remove(item3);
					cargoState.AddToCargo(item3);
				}
			}
		}
	}
}
