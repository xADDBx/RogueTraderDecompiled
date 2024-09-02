using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Controllers.Interactions;

public class InteractionFXController : IController, IInteractWithVariantActorHandler, ISubscriber
{
	void IInteractWithVariantActorHandler.HandleInteractWithVariantActor(InteractionPart interactionPart, IInteractionVariantActor variantActor)
	{
		if (variantActor.ShowInteractFx)
		{
			InteractionSettings.InteractWithToolFXData interactWithToolFXData = GetInteractFXData();
			if (interactWithToolFXData == null || !interactWithToolFXData.DoNotShowFx)
			{
				GameObject prefab = ((interactWithToolFXData != null && interactWithToolFXData.OverrideDefaultFx && interactWithToolFXData.OverrideFxPrefab != null) ? interactWithToolFXData.OverrideFxPrefab : GetDefaultInteractFx());
				GameObject target = ((interactWithToolFXData != null && interactWithToolFXData.FxLocator != null) ? interactWithToolFXData.FxLocator : interactionPart.Owner.View.Or(null)?.gameObject);
				FxHelper.SpawnFxOnGameObject(prefab, target);
			}
		}
		GameObject GetDefaultInteractFx()
		{
			if (variantActor.Type == InteractionActorType.MeltaCharge)
			{
				return Game.Instance.BlueprintRoot.Prefabs.DefaultInteractWithMeltaChargeFxPrefab;
			}
			throw new NotImplementedException();
		}
		InteractionSettings.InteractWithToolFXData GetInteractFXData()
		{
			if (variantActor.Type == InteractionActorType.MeltaCharge)
			{
				return interactionPart.Settings?.InteractWithMeltaChargeFXData;
			}
			throw new NotImplementedException();
		}
	}
}
