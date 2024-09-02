using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

public class DoorInteractionViewTrigger : MonoBehaviour, IInteractionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	public ActionsReference OpenActions;

	public ActionsReference CloseActions;

	public ActionsReference RestrictedActions;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnInteract(InteractionPart interaction)
	{
		InteractionDoorPart interactionDoorPart = interaction as InteractionDoorPart;
		if (!interactionDoorPart || !(interactionDoorPart.View.Or(null)?.gameObject == base.gameObject))
		{
			return;
		}
		ActionsHolder actionsHolder = (interactionDoorPart.IsOpen ? OpenActions : CloseActions).Get();
		if ((bool)actionsHolder)
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				actionsHolder.Actions.Run();
			}
		}
	}

	public void OnInteractionRestricted(InteractionPart interaction)
	{
		if (RestrictedActions.IsEmpty())
		{
			return;
		}
		InteractionDoorPart interactionDoorPart = interaction as InteractionDoorPart;
		if ((bool)interactionDoorPart && interactionDoorPart.View.Or(null)?.gameObject == base.gameObject)
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				RestrictedActions.Get().Actions.Run();
			}
		}
	}
}
