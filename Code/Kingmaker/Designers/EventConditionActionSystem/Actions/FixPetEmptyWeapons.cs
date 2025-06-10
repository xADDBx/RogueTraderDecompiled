using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("30ce39fc16e14ee195ce5dc9a0a6d6f5")]
public class FixPetEmptyWeapons : GameAction
{
	public override string GetCaption()
	{
		return "Fix pet`s weapon missing, set default weapon in empty slot";
	}

	protected override void RunAction()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			if (allCharacter.CombatGroup.Id == "<directly-controllable-unit>")
			{
				BaseUnitEntity pet = allCharacter.Pet;
				if (pet != null && !pet.Body.PrimaryHand.HasWeapon)
				{
					pet.Body.UpgradeHandsFromBlueprint();
				}
			}
		}
	}
}
