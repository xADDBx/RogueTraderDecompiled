using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Interaction;

[TypeId("a5ccdeff1718ec946a6b7f7de499dd64")]
public class ActionsOnClick : UnitInteractionComponent
{
	[JsonProperty]
	public ActionList Actions;

	[JsonConstructor]
	protected ActionsOnClick()
	{
	}

	public override AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		using (ContextData<InteractingUnitData>.Request().Setup(user))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				Actions.Run();
			}
		}
		return AbstractUnitCommand.ResultType.Success;
	}
}
