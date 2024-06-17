using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("643f5d612030b3e45ae8775296bbb8d5")]
public class WarhammerContextActionStarshipPBAEAttack : ContextAction
{
	public override string GetCaption()
	{
		return "Perform PBAE attack with a starship weapon";
	}

	public override void RunAction()
	{
		if (!(base.Context.MaybeCaster is StarshipEntity starshipEntity))
		{
			return;
		}
		AbilityData abilityData = base.Context.SourceAbilityContext?.Ability;
		if ((object)abilityData == null)
		{
			return;
		}
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit == starshipEntity || !(allBaseAwakeUnit is StarshipEntity starshipEntity2) || starshipEntity2.Navigation.IsSoftUnit || !abilityData.CanTarget(allBaseAwakeUnit))
			{
				continue;
			}
			RuleStarshipPerformAttack ruleStarshipPerformAttack = null;
			RuleStarshipPerformAttack ruleStarshipPerformAttack2 = null;
			List<RuleStarshipPerformAttack> list = new List<RuleStarshipPerformAttack>();
			for (int i = 0; i < starshipEntity.TeamUnitsAlive; i++)
			{
				RuleStarshipPerformAttack ruleStarshipPerformAttack3 = new RuleStarshipPerformAttack(starshipEntity, starshipEntity2, abilityData, abilityData.StarshipWeapon);
				if (ruleStarshipPerformAttack == null)
				{
					ruleStarshipPerformAttack = ruleStarshipPerformAttack3;
				}
				ruleStarshipPerformAttack3.FirstAttackInBurst = ruleStarshipPerformAttack;
				if (ruleStarshipPerformAttack2 != null)
				{
					ruleStarshipPerformAttack2.NextAttackInBurst = ruleStarshipPerformAttack3;
				}
				ruleStarshipPerformAttack2 = ruleStarshipPerformAttack3;
				list.Add(ruleStarshipPerformAttack3);
			}
			list.ForEach(delegate(RuleStarshipPerformAttack atk)
			{
				Rulebook.Trigger(atk);
			});
		}
	}
}
