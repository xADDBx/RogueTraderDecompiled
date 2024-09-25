using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("106327c1089dd0242b12633b4dc0f8e3")]
public class ChangeAccessibleStarshipInventory : GameAction
{
	[Serializable]
	public enum StarshipInventoryAvailability
	{
		Available,
		NotAvailable
	}

	[SerializeField]
	public StarshipInventoryAvailability Value;

	public override string GetCaption()
	{
		return "Change Starship Inventory Accessibility";
	}

	protected override void RunAction()
	{
		bool canAccessStarshipInventory = Value == StarshipInventoryAvailability.Available;
		Game.Instance.Player.CanAccessStarshipInventory = canAccessStarshipInventory;
		EventBus.RaiseEvent(delegate(ICanAccessStarshipInventoryHandler h)
		{
			h.HandleCanAccessStarshipInventory();
		});
	}
}
