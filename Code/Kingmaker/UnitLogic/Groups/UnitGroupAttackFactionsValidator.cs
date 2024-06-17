using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UnitLogic.Groups;

internal static class UnitGroupAttackFactionsValidator
{
	[Cheat(Name = "validate_unit_groups")]
	public static void Validate()
	{
		foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
		{
			ValidateGroup(unitGroup);
		}
	}

	private static void ValidateGroup(UnitGroup group)
	{
		if (group.AttackFactions.HasItem((BlueprintFaction i) => group.Units.HasItem((UnitReference ii) => ii.Entity?.ToBaseUnitEntity().Faction.Blueprint == i)))
		{
			PFLog.Default.Log($"Group contains enemies: {group}\n" + " - attack factions: " + string.Join(", ", group.AttackFactions.Select((BlueprintFaction i) => i.name)) + "\n - factions:  " + string.Join(", ", group.Select((BaseUnitEntity i) => i.Faction.Blueprint.name).Distinct()));
		}
	}
}
