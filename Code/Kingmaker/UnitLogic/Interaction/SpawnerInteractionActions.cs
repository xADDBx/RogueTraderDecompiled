using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Spawners;

namespace Kingmaker.UnitLogic.Interaction;

[KnowledgeDatabaseID("4dc8ec633041a694e9994df48d4e7a09")]
public class SpawnerInteractionActions : SpawnerInteraction
{
	[Obsolete]
	[ShowIf("ObsoleteActionsNotEmpty")]
	public ActionsReference Actions;

	public List<ActionsReference> ActionHolders = new List<ActionsReference>();

	private bool ObsoleteActionsNotEmpty
	{
		get
		{
			if (Actions?.Get() == null)
			{
				return false;
			}
			return Actions.Get().HasActions;
		}
	}

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
					foreach (ActionsReference actionHolder in ActionHolders)
					{
						if (actionHolder?.Get() != null)
						{
							actionHolder.Get().Actions.Run();
						}
					}
				}
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
