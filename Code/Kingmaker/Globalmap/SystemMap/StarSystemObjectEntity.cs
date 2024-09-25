using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemObjectEntity : MapObjectEntity, IStarSystemObjectEntity, IMapObjectEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	[JsonProperty]
	public List<BasePointOfInterest> PointOfInterests = new List<BasePointOfInterest>();

	[JsonProperty]
	public List<LootFromPointOfInterestHolder> LootHolder = new List<LootFromPointOfInterestHolder>();

	[JsonProperty]
	public List<CargoFromPointOfInterestHolder> CargoHolder = new List<CargoFromPointOfInterestHolder>();

	[JsonProperty]
	public Dictionary<BlueprintResource, int> ResourcesOnObject = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public bool IsScanned;

	[JsonProperty]
	public bool IsScannedOnStart;

	[JsonProperty]
	public Dictionary<BlueprintResource, ItemEntity> ResourceMiners = new Dictionary<BlueprintResource, ItemEntity>();

	public bool IsFullyExplored
	{
		get
		{
			if (IsScanned)
			{
				return PointOfInterests.All((BasePointOfInterest point) => point.Status == BasePointOfInterest.ExplorationStatus.Explored);
			}
			return false;
		}
	}

	public new BlueprintStarSystemObject Blueprint => base.Blueprint as BlueprintStarSystemObject;

	public new StarSystemObjectView View => base.View as StarSystemObjectView;

	public StarSystemObjectEntity(string uniqueId, bool isInGame, BlueprintStarSystemObject blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
		GetPointOfInterestFromBlueprint(blueprint);
		SetResources();
		IsScannedOnStart = Blueprint.IsScannedOnStart;
	}

	public StarSystemObjectEntity(StarSystemObjectView view, BlueprintStarSystemObject blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
		GetPointOfInterestFromBlueprint(blueprint);
		SetResources();
		IsScannedOnStart = Blueprint.IsScannedOnStart;
	}

	protected StarSystemObjectEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected void GetPointOfInterestFromBlueprint(BlueprintStarSystemObject blueprint)
	{
		foreach (BasePointOfInterestComponent pointComponent in blueprint.GetComponents<BasePointOfInterestComponent>().ToList().EmptyIfNull())
		{
			if (!PointOfInterests.Any((BasePointOfInterest point) => point.Blueprint == pointComponent.PointBlueprint))
			{
				BasePointOfInterest basePointOfInterest = BasePointOfInterest.CreatePointOfInterest(pointComponent.PointBlueprint);
				PointOfInterests.Add(basePointOfInterest);
				if (basePointOfInterest is PointOfInterestLoot pointOfInterestLoot)
				{
					AddLootHolder(pointOfInterestLoot, pointOfInterestLoot.Blueprint.ExplorationLoot);
				}
				if (basePointOfInterest is PointOfInterestCargo pointOfInterestCargo)
				{
					AddCargoHolder(pointOfInterestCargo, pointOfInterestCargo.Blueprint.ExplorationCargo);
				}
			}
		}
	}

	private void SetResources()
	{
		ResourceData[] array = Blueprint.Resources.EmptyIfNull();
		foreach (ResourceData resourceData in array)
		{
			ResourcesOnObject.Add(resourceData.Resource.Get(), resourceData.Count);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		foreach (LootFromPointOfInterestHolder item in LootHolder)
		{
			item.Items?.PostLoad();
		}
		foreach (ItemEntity value in ResourceMiners.Values)
		{
			value.PostLoad();
		}
		GetPointOfInterestFromBlueprint(Blueprint);
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		foreach (LootFromPointOfInterestHolder item in LootHolder)
		{
			item.Items?.PrePostLoad();
		}
		foreach (ItemEntity value in ResourceMiners.Values)
		{
			value.PrePostLoad();
		}
	}

	protected override void OnDispose()
	{
		LootHolder.ForEach(delegate(LootFromPointOfInterestHolder lh)
		{
			lh.Dispose();
		});
		base.OnDispose();
	}

	public void AddResource(BlueprintResource resource, int count)
	{
		if (count > 0)
		{
			if (ResourcesOnObject.TryGetValue(resource, out var _))
			{
				ResourcesOnObject[resource] += count;
			}
			else
			{
				ResourcesOnObject.Add(resource, count);
			}
		}
	}

	public void Scan()
	{
		IsScanned = true;
		PointOfInterests.ForEach(delegate(BasePointOfInterest point)
		{
			if (point.Status == BasePointOfInterest.ExplorationStatus.NotExplored)
			{
				point.Status = BasePointOfInterest.ExplorationStatus.NeedInteraction;
			}
		});
		OnScan();
		EventBus.RaiseEvent(this, delegate(IScanStarSystemObjectHandler h)
		{
			h.HandleScanStarSystemObject();
		});
	}

	protected virtual void OnScan()
	{
		ScanStarSystemObjectActions component = Blueprint.GetComponent<ScanStarSystemObjectActions>();
		if (component != null && (component.Conditions == null || component.Conditions.Check()))
		{
			using (ContextData<StarSystemObjectContextData>.Request().Setup(this))
			{
				component.AdditionalActions.Run();
			}
		}
	}

	public void PlayBarkBanter()
	{
		if (Blueprint.BarkBanterList?.Get() != null)
		{
			BlueprintBarkBanter bark = (from b in Blueprint.BarkBanterList.Get().GetBarkBanters()
				where b?.CanBePlayed() ?? false
				select b).EmptyIfNull().ToList().WeightedRandom(PFStatefulRandom.GlobalMap);
			EventBus.RaiseEvent(delegate(IBarkBanterPlayedHandler e)
			{
				e.HandleBarkBanter(bark);
			});
		}
	}

	public void AddLootHolder(BasePointOfInterest poi, List<LootEntry> lootCollection)
	{
		LootHolder.Add(new LootFromPointOfInterestHolder(lootCollection, this, poi));
	}

	public void AddCargoHolder(BasePointOfInterest poi, List<BlueprintCargo> cargoCollection)
	{
		CargoHolder.Add(new CargoFromPointOfInterestHolder(poi, cargoCollection, this));
	}

	public bool IsSsoConnectsToArea(BlueprintArea area)
	{
		if (area == null)
		{
			return false;
		}
		foreach (BasePointOfInterest pointOfInterest in PointOfInterests)
		{
			if (pointOfInterest is PointOfInterestGroundOperation groundPoi && IsSsoPointConnectsToArea(area, groundPoi))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSsoPointConnectsToArea(BlueprintArea area, PointOfInterestGroundOperation groundPoi)
	{
		BlueprintArea area2 = groundPoi.Blueprint.AreaEnterPoint.Area;
		BlueprintArea additionalArea = groundPoi.Blueprint.AdditionalArea;
		if (area2 != area)
		{
			return additionalArea == area;
		}
		return true;
	}

	public Dictionary<BlueprintResource, int> ResourcesFromMiners()
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		foreach (KeyValuePair<BlueprintResource, ItemEntity> resourceMiner in ResourceMiners)
		{
			if (ResourcesOnObject.TryGetValue(resourceMiner.Key, out var value))
			{
				dictionary.Add(resourceMiner.Key, ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(value));
			}
		}
		return dictionary;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<BasePointOfInterest> pointOfInterests = PointOfInterests;
		if (pointOfInterests != null)
		{
			for (int i = 0; i < pointOfInterests.Count; i++)
			{
				Hash128 val2 = ClassHasher<BasePointOfInterest>.GetHash128(pointOfInterests[i]);
				result.Append(ref val2);
			}
		}
		List<LootFromPointOfInterestHolder> lootHolder = LootHolder;
		if (lootHolder != null)
		{
			for (int j = 0; j < lootHolder.Count; j++)
			{
				Hash128 val3 = ClassHasher<LootFromPointOfInterestHolder>.GetHash128(lootHolder[j]);
				result.Append(ref val3);
			}
		}
		List<CargoFromPointOfInterestHolder> cargoHolder = CargoHolder;
		if (cargoHolder != null)
		{
			for (int k = 0; k < cargoHolder.Count; k++)
			{
				Hash128 val4 = ClassHasher<CargoFromPointOfInterestHolder>.GetHash128(cargoHolder[k]);
				result.Append(ref val4);
			}
		}
		Dictionary<BlueprintResource, int> resourcesOnObject = ResourcesOnObject;
		if (resourcesOnObject != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item in resourcesOnObject)
			{
				Hash128 hash = default(Hash128);
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val6);
				int obj = item.Value;
				Hash128 val7 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val7);
				val5 ^= hash.GetHashCode();
			}
			result.Append(ref val5);
		}
		result.Append(ref IsScanned);
		result.Append(ref IsScannedOnStart);
		Dictionary<BlueprintResource, ItemEntity> resourceMiners = ResourceMiners;
		if (resourceMiners != null)
		{
			int val8 = 0;
			foreach (KeyValuePair<BlueprintResource, ItemEntity> item2 in resourceMiners)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val9);
				Hash128 val10 = ClassHasher<ItemEntity>.GetHash128(item2.Value);
				hash2.Append(ref val10);
				val8 ^= hash2.GetHashCode();
			}
			result.Append(ref val8);
		}
		return result;
	}
}
