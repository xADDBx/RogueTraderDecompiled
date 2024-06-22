using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("b68d691de8d312942ba1cda58b61b720")]
public class WarhammerContextActionStarshipPerformAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform an attack with a starship weapon that gave this ability";
	}

	protected override void RunAction()
	{
		if (!(base.Context.MaybeCaster is StarshipEntity starshipEntity) || !(base.Target.Entity is StarshipEntity target))
		{
			return;
		}
		AbilityData ability = base.Context.SourceAbilityContext.Ability;
		if ((object)ability == null)
		{
			return;
		}
		ItemEntityStarshipWeapon itemEntityStarshipWeapon = ability?.StarshipWeapon;
		if (itemEntityStarshipWeapon != null)
		{
			for (int i = 0; i < starshipEntity.TeamUnitsAlive; i++)
			{
				Rulebook.Trigger(new RuleStarshipPerformAttack(starshipEntity, target, ability, itemEntityStarshipWeapon));
			}
		}
	}
}
