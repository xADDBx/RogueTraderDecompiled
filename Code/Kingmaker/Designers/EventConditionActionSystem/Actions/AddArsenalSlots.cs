using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("1397ef3777f84f2597dd2b9f41144f6f")]
public class AddArsenalSlots : GameAction
{
	public override string GetCaption()
	{
		return "Add arsenal slots if needed";
	}

	protected override void RunAction()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		HullSlots hullSlots = playerShip.Hull.HullSlots;
		if (hullSlots.Arsenals.Count != 2)
		{
			hullSlots.CreateArsenals(playerShip.Blueprint.HullSlots.Arsenals);
			hullSlots.EquipmentSlots.AddRange(hullSlots.Arsenals);
		}
	}
}
