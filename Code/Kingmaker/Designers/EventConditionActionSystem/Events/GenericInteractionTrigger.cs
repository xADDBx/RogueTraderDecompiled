using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/GenericInteractionTrigger")]
[AllowMultipleComponents]
[TypeId("40186c9b2cc082a4ab4ac4f0dad177c7")]
public class GenericInteractionTrigger : EntityFactComponentDelegate, IInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference MapObject;

	public ActionList Actions;

	public ActionList RestrictedActions;

	public void OnInteract(InteractionPart interaction)
	{
		if (interaction.Owner == MapObject.FindData())
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void OnInteractionRestricted(InteractionPart interaction)
	{
		if (RestrictedActions.HasActions && interaction.Owner == MapObject.FindData())
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				RestrictedActions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
