using System;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestBookEvent : BasePointOfInterest, IDialogInteractionHandler, ISubscriber, IHashable
{
	public new BlueprintPointOfInterestBookEvent Blueprint => (BlueprintPointOfInterestBookEvent)base.Blueprint;

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status != ExplorationStatus.Explored)
		{
			base.Interact(entity);
			EventBus.Subscribe(this);
			DialogData data = DialogController.SetupDialogWithoutTarget(Blueprint.BookEvent, null);
			Game.Instance.DialogController.StartDialog(data);
		}
	}

	public PointOfInterestBookEvent(BlueprintPointOfInterestBookEvent blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestBookEvent(JsonConstructorMark _)
		: base(_)
	{
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
		if (Blueprint.BookEvent == dialog && !Blueprint.IsRepeating)
		{
			Game.Instance.GameCommandQueue.PointOfInterestSetInteracted(Blueprint);
		}
		EventBus.Unsubscribe(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
