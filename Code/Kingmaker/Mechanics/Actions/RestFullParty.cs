using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Mechanics.Actions;

[TypeId("3e981548602c4e2fa67439a43c894d97")]
public class RestFullParty : ContextAction
{
	public override string GetCaption()
	{
		return "Rest full party";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			PartHealth.RestUnit(partyAndPet);
		}
		foreach (ItemEntity item in Game.Instance.Player.Inventory)
		{
			item.RestoreCharges();
		}
	}
}
