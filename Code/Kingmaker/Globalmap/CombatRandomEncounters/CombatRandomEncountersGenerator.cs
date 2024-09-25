using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

public class CombatRandomEncountersGenerator
{
	public static Dictionary<EntityReference, BlueprintUnit> GenerateArea(int seed, [NotNull] BlueprintArea area, BlueprintAreaEnterPoint enterPoint, [CanBeNull] BlueprintRandomGroupOfUnits group)
	{
		int generatedCombatRandomEncounterCount = Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount;
		CombatRandomEncounterSettings areaSettings = GetAreaSettings(area);
		PersistentRandom.Generator generator = PersistentRandom.Seed(seed).MakeGenerator("generate_enter_point", generatedCombatRandomEncounterCount);
		PersistentRandom.Generator generator2 = generator.Branch("random_unit_group", generatedCombatRandomEncounterCount);
		BlueprintRandomGroupOfUnits group2 = group ?? ((BlueprintRandomGroupOfUnits)areaSettings.UnitGroups.Random(PFStatefulRandom.GlobalMap, ((PersistentRandom.Generator)generator2).NextRange));
		List<BlueprintUnit> units = GenerateRandomUnits(generator.Branch("random_units_on_area", generatedCombatRandomEncounterCount), group2);
		return GenerateSpawnPlaces(generator.Branch("generate_random_spawners", generatedCombatRandomEncounterCount), area, enterPoint, units);
	}

	private static CombatRandomEncounterSettings GetAreaSettings(BlueprintArea area)
	{
		CombatRandomEncounterSettings combatRandomEncounterSettings = BlueprintWarhammerRoot.Instance.CombatRandomEncountersRoot.Settings.FirstOrDefault((CombatRandomEncounterSettings setting) => setting.Area.Equals(area));
		if (combatRandomEncounterSettings == null)
		{
			PFLog.Default.Warning($"Cannot find combat random encounter settings for {area}. Random generation will be skipped");
		}
		return combatRandomEncounterSettings;
	}

	public static BlueprintAreaEnterPointReference GenerateRandomEnterPoint(int seed, BlueprintArea area)
	{
		PersistentRandom.Generator generator = PersistentRandom.Seed(seed).MakeGenerator("generate_enter_point", Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount);
		return GetAreaSettings(area)?.EnterPoints.Random(PFStatefulRandom.GlobalMap, ((PersistentRandom.Generator)generator).NextRange);
	}

	private static List<BlueprintUnit> GenerateRandomUnits(PersistentRandom.Generator generator, BlueprintRandomGroupOfUnits group)
	{
		List<UnitInGroupSettings> source = (from uGroup in @group.Units.EmptyIfNull()
			where uGroup.UnitsCount != 0
			select uGroup).ToList();
		Dictionary<BlueprintUnitReference, int> dictionary = source.ToDictionary((UnitInGroupSettings uGroup) => uGroup.Unit, (UnitInGroupSettings uGroup) => uGroup.UnitsCount);
		int num = generator.NextRange(group.MinCount, group.MaxCount + 1);
		List<BlueprintUnit> list = new List<BlueprintUnit>();
		if (num == 0)
		{
			return list;
		}
		foreach (UnitInGroupSettings item in source.Where((UnitInGroupSettings unit) => unit.IsMandatoryInGroup).ToList())
		{
			if (!dictionary.TryGetValue(item.Unit, out var _))
			{
				continue;
			}
			for (int i = 0; i < item.UnitsCount; i++)
			{
				list.Add(item.Unit);
				if (list.Count == num - 1)
				{
					break;
				}
			}
			dictionary.Remove(item.Unit);
			if (list.Count == num - 1)
			{
				break;
			}
		}
		for (int j = list.Count; j < num; j++)
		{
			if (!dictionary.Any())
			{
				break;
			}
			BlueprintUnitReference key = dictionary.Random(PFStatefulRandom.GlobalMap, ((PersistentRandom.Generator)generator).NextRange).Key;
			list.Add(key.Get());
			dictionary[key]--;
			if (dictionary[key] == 0)
			{
				dictionary.Remove(key);
			}
		}
		return list;
	}

	private static Dictionary<EntityReference, BlueprintUnit> GenerateSpawnPlaces(PersistentRandom.Generator generator, BlueprintArea area, BlueprintAreaEnterPoint enterPoint, List<BlueprintUnit> units)
	{
		List<SpawnerRandomEncounterSetting> list = area.GetComponent<AreaRandomEncounter>()?.RandomSpawners.ToList();
		if (list == null)
		{
			return null;
		}
		Dictionary<EntityReference, BlueprintUnit> dictionary = new Dictionary<EntityReference, BlueprintUnit>();
		foreach (BlueprintUnit unit in units)
		{
			List<EntityReference> list2 = new List<EntityReference>();
			AddTags tags = unit.GetComponent<AddTags>();
			if (tags == null)
			{
				PFLog.Default.Warning($"Unit {unit} don't have component AddTags with random encounter role");
				continue;
			}
			foreach (SpawnerRandomEncounterSetting item in list)
			{
				if (item.RoleVariants.FirstOrDefault((UnitRolesByEnterPoint variant) => variant.EnterPoint.Get() == enterPoint && (variant.Roles & tags.RandomEncounterRole) != 0) != null)
				{
					list2.Add(item.Entity);
				}
			}
			if (list2.Any())
			{
				EntityReference randomSpawner = list2.Random(PFStatefulRandom.GlobalMap, ((PersistentRandom.Generator)generator).NextRange);
				list.Remove((SpawnerRandomEncounterSetting spawner) => spawner.Entity == randomSpawner);
				dictionary.Add(randomSpawner, unit);
			}
		}
		return dictionary;
	}

	public static EntityReference GenerateCovers(BlueprintArea area)
	{
		List<EntityReference> obj = area.GetComponent<AreaRandomEncounter>()?.CoverGroupVariations;
		int hashCode = Game.Instance.Player.GameId.GetHashCode();
		int generatedCombatRandomEncounterCount = Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount;
		return LinqExtensions.Random(randomFromRange: PersistentRandom.Seed(hashCode).MakeGenerator("generate_covers_group", generatedCombatRandomEncounterCount).NextRange, enumerable: obj?, statefulRandom: PFStatefulRandom.GlobalMap);
	}

	public static EntityReference GenerateTraps(BlueprintArea area)
	{
		List<EntityReference> obj = area.GetComponent<AreaRandomEncounter>()?.TrapGroupVariations;
		int hashCode = Game.Instance.Player.GameId.GetHashCode();
		int generatedCombatRandomEncounterCount = Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount;
		return LinqExtensions.Random(randomFromRange: PersistentRandom.Seed(hashCode).MakeGenerator("generate_traps_group", generatedCombatRandomEncounterCount).NextRange, enumerable: obj?, statefulRandom: PFStatefulRandom.GlobalMap);
	}

	public static EntityReference GenerateAreaEffects(BlueprintArea area)
	{
		List<EntityReference> obj = area.GetComponent<AreaRandomEncounter>()?.AreaEffectGroupVariations;
		int hashCode = Game.Instance.Player.GameId.GetHashCode();
		int generatedCombatRandomEncounterCount = Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount;
		return LinqExtensions.Random(randomFromRange: PersistentRandom.Seed(hashCode).MakeGenerator("generate_area_effects_group", generatedCombatRandomEncounterCount).NextRange, enumerable: obj?, statefulRandom: PFStatefulRandom.GlobalMap);
	}

	public static EntityReference GenerateOtherMapObjects(BlueprintArea area)
	{
		List<EntityReference> obj = area.GetComponent<AreaRandomEncounter>()?.OtherMapObjectGroupVariations;
		int hashCode = Game.Instance.Player.GameId.GetHashCode();
		int generatedCombatRandomEncounterCount = Game.Instance.Player.CombatRandomEncounterState.GeneratedCombatRandomEncounterCount;
		return LinqExtensions.Random(randomFromRange: PersistentRandom.Seed(hashCode).MakeGenerator("generate_other_map_objects_group", generatedCombatRandomEncounterCount).NextRange, enumerable: obj?, statefulRandom: PFStatefulRandom.GlobalMap);
	}
}
