using Core.Cheats;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("78ca01ac1e0f2e541b501ed2c46e6b8a")]
public class EtudeBracketForbidOpenShipInventory : EtudeBracketTrigger, IHashable
{
	protected override void OnEnter()
	{
		Game.Instance.Player.CanAccessStarshipInventory = false;
		EventBus.RaiseEvent(delegate(ICanAccessStarshipInventoryHandler h)
		{
			h.HandleCanAccessStarshipInventory();
		});
	}

	protected override void OnExit()
	{
		Game.Instance.Player.CanAccessStarshipInventory = true;
		EventBus.RaiseEvent(delegate(ICanAccessStarshipInventoryHandler h)
		{
			h.HandleCanAccessStarshipInventory();
		});
	}

	[Cheat(Name = "change_ship_access")]
	public static void ChangeStarshipAccess()
	{
		Game.Instance.Player.CanAccessStarshipInventory = !Game.Instance.Player.CanAccessStarshipInventory;
		EventBus.RaiseEvent(delegate(ICanAccessStarshipInventoryHandler h)
		{
			h.HandleCanAccessStarshipInventory();
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
