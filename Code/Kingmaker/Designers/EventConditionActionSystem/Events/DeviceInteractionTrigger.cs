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
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/DeviceInteractionTrigger")]
[AllowMultipleComponents]
[TypeId("b7487196181956f4ea415cc28ea89dc3")]
public class DeviceInteractionTrigger : EntityFactComponentDelegate, IInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public ActionList Actions;

	public ActionList RestrictedActions;

	public void OnInteract(InteractionPart interaction)
	{
		MapObjectView mapObjectView = interaction.View.Or(null);
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void OnInteractionRestricted(InteractionPart interaction)
	{
		if (!RestrictedActions.HasActions)
		{
			return;
		}
		MapObjectView mapObjectView = interaction.View.Or(null);
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
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
