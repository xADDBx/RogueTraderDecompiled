using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class DetectedVendorData : IHashable
{
	[JsonProperty]
	public EntityRef<MechanicEntity> Entity { get; private set; }

	[JsonProperty]
	public BlueprintScriptableObject EntityBlueprint { get; private set; }

	[JsonProperty]
	public BlueprintVendorFaction.Reference Faction { get; private set; }

	[JsonProperty]
	public BlueprintAreaReference Area { get; private set; }

	[JsonProperty]
	public BlueprintAreaPartReference AreaPart { get; private set; }

	[JsonProperty]
	public int Chapter { get; private set; }

	public string VendorName
	{
		get
		{
			object obj = (EntityBlueprint as BlueprintUnit)?.CharacterName;
			if (obj == null)
			{
				BlueprintMechanicEntityFact obj2 = EntityBlueprint as BlueprintMechanicEntityFact;
				if (obj2 == null)
				{
					return null;
				}
				obj = obj2.Name;
			}
			return (string)obj;
		}
	}

	public DetectedVendorData(MechanicEntity entity, [CanBeNull] BlueprintArea area, [CanBeNull] BlueprintAreaPart areaPart, int chapter)
	{
		Entity = entity;
		EntityBlueprint = entity.Blueprint;
		Faction = entity.GetRequired<PartVendor>().Faction?.ToReference<BlueprintVendorFaction.Reference>();
		Area = area.ToReference<BlueprintAreaReference>();
		AreaPart = areaPart.ToReference<BlueprintAreaPartReference>();
		Chapter = chapter;
	}

	[JsonConstructor]
	private DetectedVendorData()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = Entity;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(EntityBlueprint);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(Faction);
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(Area);
		result.Append(ref val4);
		Hash128 val5 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(AreaPart);
		result.Append(ref val5);
		int val6 = Chapter;
		result.Append(ref val6);
		return result;
	}
}
