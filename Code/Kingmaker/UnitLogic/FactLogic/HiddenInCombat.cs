using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("14d2de2955ae47d4b6a57d939deec347")]
public class HiddenInCombat : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool IsAlreadyHidden;
	}

	protected override void OnActivateOrPostLoad()
	{
		UpdateHidden(isDeactivating: false);
	}

	protected override void OnDeactivate()
	{
		UpdateHidden(isDeactivating: true);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		UpdateHidden(isDeactivating: false);
	}

	private void UpdateHidden(bool isDeactivating)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!componentData.IsAlreadyHidden && Game.Instance.TurnController.TurnBasedModeActive)
		{
			base.Owner.Features.Hidden.Retain();
			componentData.IsAlreadyHidden = true;
		}
		else if (componentData.IsAlreadyHidden && (!Game.Instance.TurnController.TurnBasedModeActive || isDeactivating))
		{
			base.Owner.Features.Hidden.Release();
			componentData.IsAlreadyHidden = false;
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
