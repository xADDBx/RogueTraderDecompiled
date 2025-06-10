using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("2fb419a8bdeb426694be03c94cad5d57")]
public class FixPetAttributesAfterRecruit : GameAction
{
	public override string GetCaption()
	{
		return "Fix pet attributes after master recruit";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (!(allCharacter.CombatGroup.Id == "<directly-controllable-unit>"))
			{
				continue;
			}
			BaseUnitEntity pet = allCharacter.Pet;
			if (pet != null)
			{
				pet.CombatGroup.Id = allCharacter.CombatGroup.Id;
				pet.Faction.Set(allCharacter.Faction.Blueprint);
				if (!allCharacter.Inventory.HasOwnInventory)
				{
					pet.Inventory.MakeSharedInventory();
				}
			}
		}
	}
}
