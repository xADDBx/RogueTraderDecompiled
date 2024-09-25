using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.Tutorial.Solvers;

public static class PartySpellsEnumerator
{
	public static IEnumerator<AbilityData> Get(bool withAbilities)
	{
		List<BaseUnitEntity> allCharactersAndStarships = Game.Instance.Player.AllCharactersAndStarships;
		foreach (BaseUnitEntity item in allCharactersAndStarships)
		{
			if (!withAbilities)
			{
				continue;
			}
			foreach (Ability ability in item.Abilities)
			{
				yield return ability.Data;
			}
		}
	}
}
