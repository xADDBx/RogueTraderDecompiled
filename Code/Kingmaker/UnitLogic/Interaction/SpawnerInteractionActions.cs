using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Spawners;

namespace Kingmaker.UnitLogic.Interaction;

public class SpawnerInteractionActions : SpawnerInteraction
{
	public ActionsReference Actions;

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				using (ContextData<SpawnedUnitData>.Request().Setup(GetComponent<UnitSpawnerBase>().SpawnedUnit, GetComponent<UnitSpawnerBase>().SpawnedUnit.HoldingState))
				{
					if (Actions?.Get() != null)
					{
						Actions.Get().Actions.Run();
					}
				}
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
