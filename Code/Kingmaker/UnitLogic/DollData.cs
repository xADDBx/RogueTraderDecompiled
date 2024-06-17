using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.View;
using Kingmaker.Visual.CharacterSystem;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class DollData : IHashable
{
	[JsonProperty]
	public Gender Gender;

	[CanBeNull]
	[JsonProperty]
	public BlueprintRaceVisualPreset RacePreset;

	[NotNull]
	[JsonProperty]
	public List<string> EquipmentEntityIds = new List<string>();

	[NotNull]
	[JsonProperty]
	public Dictionary<string, int> EntityRampIdices = new Dictionary<string, int>();

	[NotNull]
	[JsonProperty]
	public Dictionary<string, int> EntitySecondaryRampIdices = new Dictionary<string, int>();

	[JsonProperty]
	public bool LeftHanded;

	[JsonProperty]
	public int ClothesPrimaryIndex = -1;

	[JsonProperty]
	public int ClothesSecondaryIndex = -1;

	[NotNull]
	public UnitEntityView CreateUnitView(bool savedEquipment = false)
	{
		BlueprintCharGenRoot charGenRoot = BlueprintRoot.Instance.CharGenRoot;
		Character character = ((Gender == Gender.Male) ? charGenRoot.MaleDoll : charGenRoot.FemaleDoll);
		UnitEntityView component = character.GetComponent<UnitEntityView>();
		if (component == null)
		{
			throw new Exception($"Could not create unit view by doll data: invalid prefab {character}");
		}
		UnitEntityView unitEntityView = UnityEngine.Object.Instantiate(component);
		Character component2 = unitEntityView.GetComponent<Character>();
		if (component2 == null)
		{
			return unitEntityView;
		}
		component2.RemoveAllEquipmentEntities(savedEquipment);
		if (RacePreset != null)
		{
			component2.Skeleton = ((Gender == Gender.Male) ? RacePreset.MaleSkeleton : RacePreset.FemaleSkeleton);
			component2.AddEquipmentEntities(RacePreset.Skin.Load(Gender, RacePreset.RaceId), savedEquipment);
		}
		foreach (string equipmentEntityId in EquipmentEntityIds)
		{
			EquipmentEntity ee = ResourcesLibrary.TryGetResource<EquipmentEntity>(equipmentEntityId);
			component2.AddEquipmentEntity(ee, savedEquipment);
		}
		ApplyRampIndices(component2, savedEquipment);
		return unitEntityView;
	}

	public void ApplyRampIndices(Character avatar, bool savedEquipment = false)
	{
		foreach (KeyValuePair<string, int> entityRampIdix in EntityRampIdices)
		{
			EquipmentEntity ee = ResourcesLibrary.TryGetResource<EquipmentEntity>(entityRampIdix.Key);
			int value = entityRampIdix.Value;
			if (value >= 0)
			{
				avatar.SetPrimaryRampIndex(ee, value, savedEquipment);
			}
		}
		foreach (KeyValuePair<string, int> entitySecondaryRampIdix in EntitySecondaryRampIdices)
		{
			EquipmentEntity ee2 = ResourcesLibrary.TryGetResource<EquipmentEntity>(entitySecondaryRampIdix.Key);
			int value2 = entitySecondaryRampIdix.Value;
			if (value2 >= 0)
			{
				avatar.SetSecondaryRampIndex(ee2, value2, savedEquipment);
			}
		}
	}

	public void ConvertClothesIndices(BaseUnitEntity unit)
	{
		if (RacePreset == null)
		{
			return;
		}
		foreach (ClassData @class in unit.Progression.Classes)
		{
			foreach (EquipmentEntityLink clothesLink in @class.CharacterClass.GetClothesLinks(Gender, RacePreset.RaceId))
			{
				if (EntityRampIdices.ContainsKey(clothesLink.AssetId))
				{
					ClothesPrimaryIndex = EntityRampIdices[clothesLink.AssetId];
				}
				if (EntitySecondaryRampIdices.ContainsKey(clothesLink.AssetId))
				{
					ClothesSecondaryIndex = EntitySecondaryRampIdices[clothesLink.AssetId];
				}
			}
		}
	}

	public void PreloadEquipmentEntities()
	{
		if (RacePreset != null)
		{
			RacePreset.Skin.Preload(Gender, RacePreset.RaceId);
		}
		foreach (string equipmentEntityId in EquipmentEntityIds)
		{
			ResourcesLibrary.PreloadResource<EquipmentEntity>(equipmentEntityId);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Gender);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(RacePreset);
		result.Append(ref val);
		List<string> equipmentEntityIds = EquipmentEntityIds;
		if (equipmentEntityIds != null)
		{
			for (int i = 0; i < equipmentEntityIds.Count; i++)
			{
				Hash128 val2 = StringHasher.GetHash128(equipmentEntityIds[i]);
				result.Append(ref val2);
			}
		}
		Dictionary<string, int> entityRampIdices = EntityRampIdices;
		if (entityRampIdices != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<string, int> item in entityRampIdices)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = StringHasher.GetHash128(item.Key);
				hash.Append(ref val4);
				int obj = item.Value;
				Hash128 val5 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		Dictionary<string, int> entitySecondaryRampIdices = EntitySecondaryRampIdices;
		if (entitySecondaryRampIdices != null)
		{
			int val6 = 0;
			foreach (KeyValuePair<string, int> item2 in entitySecondaryRampIdices)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val7 = StringHasher.GetHash128(item2.Key);
				hash2.Append(ref val7);
				int obj2 = item2.Value;
				Hash128 val8 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val8);
				val6 ^= hash2.GetHashCode();
			}
			result.Append(ref val6);
		}
		result.Append(ref LeftHanded);
		result.Append(ref ClothesPrimaryIndex);
		result.Append(ref ClothesSecondaryIndex);
		return result;
	}
}
