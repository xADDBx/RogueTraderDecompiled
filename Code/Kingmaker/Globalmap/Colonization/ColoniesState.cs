using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class ColoniesState : IHashable
{
	public class ColonyData : IHashable
	{
		[JsonProperty]
		public BlueprintStarSystemMap Area;

		[JsonProperty]
		public BlueprintPlanet Planet;

		[JsonProperty]
		public Colony Colony;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Area);
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Planet);
			result.Append(ref val2);
			Hash128 val3 = ClassHasher<Colony>.GetHash128(Colony);
			result.Append(ref val3);
			return result;
		}
	}

	public class MinerData : IHashable
	{
		[JsonProperty]
		public BlueprintStarSystemObject Sso;

		[JsonProperty]
		public BlueprintResource Resource;

		[JsonProperty]
		public int InitialCount;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Sso);
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Resource);
			result.Append(ref val2);
			result.Append(ref InitialCount);
			return result;
		}
	}

	[JsonProperty]
	public readonly List<ColonyData> Colonies = new List<ColonyData>();

	[JsonProperty]
	public readonly Dictionary<BlueprintResource, int> ResourcesNotFromColonies = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public readonly HashSet<BlueprintResource> UniqueResourcesAlreadySpawned = new HashSet<BlueprintResource>();

	[JsonProperty]
	public readonly CountableFlag ForbidColonization = new CountableFlag();

	[JsonProperty]
	public readonly List<BlueprintColonyTrait> TraitsForAllColonies = new List<BlueprintColonyTrait>();

	[JsonProperty]
	private bool m_IsInitialized;

	[JsonProperty]
	public ColonyStat MinerProductivity;

	[JsonProperty]
	public List<MinerData> Miners = new List<MinerData>();

	[JsonProperty]
	public List<BlueprintQuestContract> OrdersUseResources = new List<BlueprintQuestContract>();

	[JsonProperty]
	public readonly List<ColonyStatModifier> ContentmentModifiersForAllColonies = new List<ColonyStatModifier>();

	[JsonProperty]
	public readonly List<ColonyStatModifier> SecurityModifiersForAllColonies = new List<ColonyStatModifier>();

	[JsonProperty]
	public readonly List<ColonyStatModifier> EfficiencyModifiersForAllColonies = new List<ColonyStatModifier>();

	[JsonProperty]
	public readonly Dictionary<BlueprintResource, int> GlobalResourceShortage = new Dictionary<BlueprintResource, int>();

	public void Initialize()
	{
		foreach (ColonyData colony in Colonies)
		{
			ItemsCollection items = colony.Colony.LootToReceive.Items;
			if (items == null)
			{
				continue;
			}
			foreach (ItemEntity item in items.Items)
			{
				item.Collection = items;
			}
		}
		if (!m_IsInitialized)
		{
			BlueprintColonyRoot colonyRoot = BlueprintWarhammerRoot.Instance.ColonyRoot;
			if (MinerProductivity == null)
			{
				MinerProductivity = new ColonyStat
				{
					InitialValue = colonyRoot.InitialMinerProductivity,
					MinValue = colonyRoot.MinMinerProductivity,
					MaxValue = colonyRoot.MaxMinerProductivity
				};
			}
			m_IsInitialized = true;
		}
	}

	public void PostLoad()
	{
		foreach (ColonyData colony in Colonies)
		{
			colony.Colony.PostLoad();
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<ColonyData> colonies = Colonies;
		if (colonies != null)
		{
			for (int i = 0; i < colonies.Count; i++)
			{
				Hash128 val = ClassHasher<ColonyData>.GetHash128(colonies[i]);
				result.Append(ref val);
			}
		}
		Dictionary<BlueprintResource, int> resourcesNotFromColonies = ResourcesNotFromColonies;
		if (resourcesNotFromColonies != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item in resourcesNotFromColonies)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				int obj = item.Value;
				Hash128 val4 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		HashSet<BlueprintResource> uniqueResourcesAlreadySpawned = UniqueResourcesAlreadySpawned;
		if (uniqueResourcesAlreadySpawned != null)
		{
			int num = 0;
			foreach (BlueprintResource item2 in uniqueResourcesAlreadySpawned)
			{
				num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2).GetHashCode();
			}
			result.Append(num);
		}
		Hash128 val5 = ClassHasher<CountableFlag>.GetHash128(ForbidColonization);
		result.Append(ref val5);
		List<BlueprintColonyTrait> traitsForAllColonies = TraitsForAllColonies;
		if (traitsForAllColonies != null)
		{
			for (int j = 0; j < traitsForAllColonies.Count; j++)
			{
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(traitsForAllColonies[j]);
				result.Append(ref val6);
			}
		}
		result.Append(ref m_IsInitialized);
		Hash128 val7 = ClassHasher<ColonyStat>.GetHash128(MinerProductivity);
		result.Append(ref val7);
		List<MinerData> miners = Miners;
		if (miners != null)
		{
			for (int k = 0; k < miners.Count; k++)
			{
				Hash128 val8 = ClassHasher<MinerData>.GetHash128(miners[k]);
				result.Append(ref val8);
			}
		}
		List<BlueprintQuestContract> ordersUseResources = OrdersUseResources;
		if (ordersUseResources != null)
		{
			for (int l = 0; l < ordersUseResources.Count; l++)
			{
				Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ordersUseResources[l]);
				result.Append(ref val9);
			}
		}
		List<ColonyStatModifier> contentmentModifiersForAllColonies = ContentmentModifiersForAllColonies;
		if (contentmentModifiersForAllColonies != null)
		{
			for (int m = 0; m < contentmentModifiersForAllColonies.Count; m++)
			{
				Hash128 val10 = ClassHasher<ColonyStatModifier>.GetHash128(contentmentModifiersForAllColonies[m]);
				result.Append(ref val10);
			}
		}
		List<ColonyStatModifier> securityModifiersForAllColonies = SecurityModifiersForAllColonies;
		if (securityModifiersForAllColonies != null)
		{
			for (int n = 0; n < securityModifiersForAllColonies.Count; n++)
			{
				Hash128 val11 = ClassHasher<ColonyStatModifier>.GetHash128(securityModifiersForAllColonies[n]);
				result.Append(ref val11);
			}
		}
		List<ColonyStatModifier> efficiencyModifiersForAllColonies = EfficiencyModifiersForAllColonies;
		if (efficiencyModifiersForAllColonies != null)
		{
			for (int num2 = 0; num2 < efficiencyModifiersForAllColonies.Count; num2++)
			{
				Hash128 val12 = ClassHasher<ColonyStatModifier>.GetHash128(efficiencyModifiersForAllColonies[num2]);
				result.Append(ref val12);
			}
		}
		Dictionary<BlueprintResource, int> globalResourceShortage = GlobalResourceShortage;
		if (globalResourceShortage != null)
		{
			int val13 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item3 in globalResourceShortage)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3.Key);
				hash2.Append(ref val14);
				int obj2 = item3.Value;
				Hash128 val15 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val15);
				val13 ^= hash2.GetHashCode();
			}
			result.Append(ref val13);
		}
		return result;
	}
}
