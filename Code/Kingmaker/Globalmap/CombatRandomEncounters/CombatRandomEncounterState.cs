using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

public class CombatRandomEncounterState : IHashable
{
	[JsonProperty]
	public int GeneratedCombatRandomEncounterCount;

	[JsonProperty]
	public bool IsInCombatRandomEncounter;

	[JsonProperty]
	public BlueprintArea Area;

	[JsonProperty]
	public BlueprintAreaEnterPoint EnterPoint;

	[JsonProperty]
	public Dictionary<EntityReference, BlueprintUnit> Spawners = new Dictionary<EntityReference, BlueprintUnit>();

	[JsonProperty]
	public EntityReference CoverGroup;

	[JsonProperty]
	public EntityReference TrapGroup;

	[JsonProperty]
	public EntityReference AreaEffectGroup;

	[JsonProperty]
	public EntityReference OtherMapObjectGroup;

	[JsonProperty]
	public Dictionary<BlueprintAreaEnterPoint, EntityReference> AllySpawnersInAllAreas = new Dictionary<BlueprintAreaEnterPoint, EntityReference>();

	[JsonProperty]
	public BlueprintUnit AllyBlueprint;

	[JsonProperty]
	public BlueprintUnlockableFlag UnlockFlag;

	public void ClearState()
	{
		Area = null;
		Spawners.Clear();
		EnterPoint = null;
		IsInCombatRandomEncounter = false;
		CoverGroup = null;
		TrapGroup = null;
		AreaEffectGroup = null;
		OtherMapObjectGroup = null;
		UnlockFlag = null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref GeneratedCombatRandomEncounterCount);
		result.Append(ref IsInCombatRandomEncounter);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Area);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(EnterPoint);
		result.Append(ref val2);
		Dictionary<EntityReference, BlueprintUnit> spawners = Spawners;
		if (spawners != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<EntityReference, BlueprintUnit> item in spawners)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = ClassHasher<EntityReference>.GetHash128(item.Key);
				hash.Append(ref val4);
				Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Value);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		Hash128 val6 = ClassHasher<EntityReference>.GetHash128(CoverGroup);
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<EntityReference>.GetHash128(TrapGroup);
		result.Append(ref val7);
		Hash128 val8 = ClassHasher<EntityReference>.GetHash128(AreaEffectGroup);
		result.Append(ref val8);
		Hash128 val9 = ClassHasher<EntityReference>.GetHash128(OtherMapObjectGroup);
		result.Append(ref val9);
		Dictionary<BlueprintAreaEnterPoint, EntityReference> allySpawnersInAllAreas = AllySpawnersInAllAreas;
		if (allySpawnersInAllAreas != null)
		{
			int val10 = 0;
			foreach (KeyValuePair<BlueprintAreaEnterPoint, EntityReference> item2 in allySpawnersInAllAreas)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val11);
				Hash128 val12 = ClassHasher<EntityReference>.GetHash128(item2.Value);
				hash2.Append(ref val12);
				val10 ^= hash2.GetHashCode();
			}
			result.Append(ref val10);
		}
		Hash128 val13 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(AllyBlueprint);
		result.Append(ref val13);
		Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(UnlockFlag);
		result.Append(ref val14);
		return result;
	}
}
