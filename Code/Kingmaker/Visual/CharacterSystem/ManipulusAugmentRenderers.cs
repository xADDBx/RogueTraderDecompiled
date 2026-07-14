using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class ManipulusAugmentRenderers : MonoBehaviour, IEntitySubscriber, IViewAttachedHandler, ISubscriber<IEntity>, ISubscriber, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IEventTag<IUnitEquipmentHandler, EntitySubscriber>, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IEntityLostFactHandler<EntitySubscriber>, IEntityLostFactHandler, IEventTag<IEntityLostFactHandler, EntitySubscriber>
{
	[Serializable]
	public class Variant
	{
		public string DebugName;

		public BlueprintItemAugmentReference[] AnyOfItems;

		public BlueprintFeatureReference[] AnyOfFeatures;

		public SkinnedMeshRenderer[] Renderers;
	}

	[Serializable]
	public class VariantGroup
	{
		public string DebugName;

		public SkinnedMeshRenderer[] DefaultRenderers;

		public Variant[] Variants;
	}

	[Serializable]
	public class WeaponBinding
	{
		public string DebugName;

		public BlueprintItemAugmentReference[] AnyOfItems;

		public BlueprintFeatureReference[] AnyOfFeatures;

		public GameObject WeaponPrefab;

		public Transform AttachBone;
	}

	[SerializeField]
	private VariantGroup[] _variantGroups;

	[SerializeField]
	private WeaponBinding[] _weaponBindings;

	private AbstractUnitEntityView _view;

	private bool _entityBound;

	private readonly Dictionary<WeaponBinding, GameObject> _spawnedWeapons = new Dictionary<WeaponBinding, GameObject>();

	private void Awake()
	{
		_view = GetComponentInParent<AbstractUnitEntityView>();
		HideAllShowDefaults();
		LogChannel techArt = PFLog.TechArt;
		string[] obj = new string[5]
		{
			"[augmentManipulus] Awake: view=",
			_view?.name ?? "null",
			", ",
			null,
			null
		};
		VariantGroup[] variantGroups = _variantGroups;
		object arg = ((variantGroups != null) ? variantGroups.Length : 0);
		WeaponBinding[] weaponBindings = _weaponBindings;
		obj[3] = $"variantGroups={arg}, weaponBindings={((weaponBindings != null) ? weaponBindings.Length : 0)}, ";
		obj[4] = $"dataReady={_view?.Data != null}";
		techArt.Log(string.Concat(obj));
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		bool flag = _view?.Data != null;
		PFLog.TechArt.Log($"[augmentManipulus] OnEnable: subscribed, dataReady={flag}");
		if (flag)
		{
			RebindToEntity();
		}
	}

	private void Start()
	{
		bool flag = !_entityBound && _view?.Data != null;
		PFLog.TechArt.Log($"[augmentManipulus] Start: entityBound={_entityBound}, dataReady={_view?.Data != null}, " + $"willRebind={flag}");
		if (flag)
		{
			RebindToEntity();
		}
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
		_entityBound = false;
		PFLog.TechArt.Log("[augmentManipulus] OnDisable: unsubscribed");
	}

	public IEntity GetSubscribingEntity()
	{
		return _view?.Data;
	}

	public void OnViewAttached(IEntityViewBase view)
	{
		bool flag = view == _view;
		PFLog.TechArt.Log("[augmentManipulus] OnViewAttached: paramView=" + ((view as Component)?.name ?? view?.ToString() ?? "null") + ", " + $"sameAsCached={flag}, alreadyBound={_entityBound}");
		if (flag && !_entityBound)
		{
			RebindToEntity();
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		bool flag = slot is AugmentSlot;
		PFLog.TechArt.Log("[augmentManipulus] HandleEquipmentSlotUpdated: slot=" + (slot?.GetType().Name ?? "null") + ", " + string.Format("isAugSlot={0}, new={1}, ", flag, (slot as AugmentSlot)?.ItemBlueprint?.name ?? "null") + "prev=" + ((previousItem?.Blueprint as BlueprintItemAugment)?.name ?? "null"));
		if (slot is AugmentSlot { ItemBlueprint: var itemBlueprint })
		{
			BlueprintItemAugment prevItem = previousItem?.Blueprint as BlueprintItemAugment;
			HandleAugmentChange(itemBlueprint, prevItem);
		}
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		HandleFactChange(fact, gained: true);
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		HandleFactChange(fact, gained: false);
	}

	private void HandleFactChange(EntityFact fact, bool gained)
	{
		if (fact is Feature feature)
		{
			ApplyGroupsForFeature(feature.Blueprint, gained);
		}
	}

	private void RebindToEntity()
	{
		EventBus.Unsubscribe(this);
		EventBus.Subscribe(this);
		_entityBound = true;
		IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> readOnlyDictionary = _view?.Data?.GetBodyOptional()?.Augments?.Slots;
		PFLog.TechArt.Log("[augmentManipulus] RebindToEntity: entity=" + (_view?.Data?.UniqueId ?? "null") + ", " + $"slotsCount={readOnlyDictionary?.Count ?? (-1)}");
		ApplyFullSync();
	}

	private void HideAllShowDefaults()
	{
		if (_variantGroups != null)
		{
			VariantGroup[] variantGroups = _variantGroups;
			for (int i = 0; i < variantGroups.Length; i++)
			{
				HideVariantsShowDefault(variantGroups[i]);
			}
		}
		DestroyAllSpawnedWeapons();
	}

	private static void HideVariantsShowDefault(VariantGroup group)
	{
		if (group.Variants != null)
		{
			Variant[] variants = group.Variants;
			for (int i = 0; i < variants.Length; i++)
			{
				SetRenderersActive(variants[i]?.Renderers, active: false);
			}
		}
		SetRenderersActive(group.DefaultRenderers, active: true);
	}

	private void DestroyAllSpawnedWeapons()
	{
		if (_weaponBindings == null)
		{
			return;
		}
		_spawnedWeapons.Clear();
		WeaponBinding[] weaponBindings = _weaponBindings;
		foreach (WeaponBinding weaponBinding in weaponBindings)
		{
			if (weaponBinding.AttachBone == null)
			{
				continue;
			}
			for (int num = weaponBinding.AttachBone.childCount - 1; num >= 0; num--)
			{
				Transform child = weaponBinding.AttachBone.GetChild(num);
				if (child != null)
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
		}
	}

	private void ApplyFullSync()
	{
		ApplyFullSync(_view?.Data);
	}

	public void ApplyForUnit([CanBeNull] AbstractUnitEntity unit)
	{
		PFLog.TechArt.Log("[augmentManipulus] ApplyForUnit (from DollRoom): unit=" + (unit?.UniqueId ?? "null"));
		HideAllShowDefaults();
		ApplyFullSync(unit);
	}

	private void ApplyFullSync([CanBeNull] AbstractUnitEntity unit)
	{
		IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> readOnlyDictionary = unit?.GetBodyOptional()?.Augments?.Slots;
		LogChannel techArt = PFLog.TechArt;
		string[] obj = new string[5]
		{
			"[augmentManipulus] ApplyFullSync: unit=",
			unit?.UniqueId ?? "null",
			", ",
			null,
			null
		};
		object arg = readOnlyDictionary?.Count ?? (-1);
		VariantGroup[] variantGroups = _variantGroups;
		obj[3] = $"slotsCount={arg}, variantGroups={((variantGroups != null) ? variantGroups.Length : 0)}, ";
		WeaponBinding[] weaponBindings = _weaponBindings;
		obj[4] = $"weaponBindings={((weaponBindings != null) ? weaponBindings.Length : 0)}";
		techArt.Log(string.Concat(obj));
		if (unit == null)
		{
			return;
		}
		if (_variantGroups != null)
		{
			VariantGroup[] variantGroups2 = _variantGroups;
			for (int i = 0; i < variantGroups2.Length; i++)
			{
				ApplyVariantGroup(variantGroups2[i], unit, readOnlyDictionary);
			}
		}
		ReevaluateAllWeapons(unit, readOnlyDictionary);
	}

	private void HandleAugmentChange(BlueprintItemAugment newItem, BlueprintItemAugment prevItem)
	{
		AbstractUnitEntity abstractUnitEntity = _view?.Data;
		if (abstractUnitEntity == null)
		{
			return;
		}
		IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots = abstractUnitEntity.GetBodyOptional()?.Augments?.Slots;
		int num = 0;
		if (_variantGroups != null)
		{
			VariantGroup[] variantGroups = _variantGroups;
			foreach (VariantGroup group in variantGroups)
			{
				if (GroupReferencesItem(group, newItem) || GroupReferencesItem(group, prevItem))
				{
					ApplyVariantGroup(group, abstractUnitEntity, slots);
					num++;
				}
			}
		}
		ReevaluateAllWeapons(abstractUnitEntity, slots);
		PFLog.TechArt.Log("[augmentManipulus] HandleAugmentChange: new=" + (newItem?.name ?? "null") + ", " + string.Format("prev={0}, affectedGroups={1}", prevItem?.name ?? "null", num));
	}

	private void ApplyGroupsForFeature(BlueprintFeature featureBp, bool gained)
	{
		AbstractUnitEntity abstractUnitEntity = _view?.Data;
		if (abstractUnitEntity == null)
		{
			return;
		}
		IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots = abstractUnitEntity.GetBodyOptional()?.Augments?.Slots;
		int num = 0;
		if (_variantGroups != null)
		{
			VariantGroup[] variantGroups = _variantGroups;
			foreach (VariantGroup group in variantGroups)
			{
				if (GroupReferencesFeature(group, featureBp))
				{
					ApplyVariantGroup(group, abstractUnitEntity, slots);
					num++;
				}
			}
		}
		ReevaluateAllWeapons(abstractUnitEntity, slots);
		PFLog.TechArt.Log("[augmentManipulus] HandleFact" + (gained ? "Gain" : "Lost") + ": feature=" + (featureBp?.name ?? "null") + ", " + $"affectedGroups={num}");
	}

	private static void ApplyVariantGroup(VariantGroup group, AbstractUnitEntity unit, [CanBeNull] IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots)
	{
		Variant variant = null;
		string matchedBy = null;
		if (group.Variants != null)
		{
			Variant[] variants = group.Variants;
			foreach (Variant variant2 in variants)
			{
				if (TryMatchVariant(variant2, unit, slots, out matchedBy))
				{
					variant = variant2;
					break;
				}
			}
		}
		if (group.Variants != null)
		{
			Variant[] variants = group.Variants;
			for (int i = 0; i < variants.Length; i++)
			{
				SetRenderersActive(variants[i]?.Renderers, active: false);
			}
		}
		SetRenderersActive(group.DefaultRenderers, variant == null);
		if (variant != null)
		{
			SetRenderersActive(variant.Renderers, active: true);
		}
		PFLog.TechArt.Log("[augmentManipulus] ApplyVariantGroup group='" + group.DebugName + "' active='" + (variant?.DebugName ?? "default") + "' matchedBy='" + (matchedBy ?? "—") + "'");
	}

	private void ReevaluateAllWeapons(AbstractUnitEntity unit, [CanBeNull] IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots)
	{
		if (_weaponBindings == null)
		{
			return;
		}
		for (int i = 0; i < _weaponBindings.Length; i++)
		{
			WeaponBinding weaponBinding = _weaponBindings[i];
			if (weaponBinding.AttachBone == null)
			{
				continue;
			}
			string matchedBy;
			bool flag = TryMatchTriggers(weaponBinding.AnyOfItems, weaponBinding.AnyOfFeatures, unit, slots, out matchedBy);
			if (flag)
			{
				for (int j = 0; j < i; j++)
				{
					WeaponBinding weaponBinding2 = _weaponBindings[j];
					if (!(weaponBinding2.AttachBone != weaponBinding.AttachBone) && TryMatchTriggers(weaponBinding2.AnyOfItems, weaponBinding2.AnyOfFeatures, unit, slots, out var _))
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				SpawnWeapon(weaponBinding, matchedBy);
			}
			else
			{
				DestroySpawnedWeapon(weaponBinding);
			}
		}
	}

	private static bool TryMatchVariant(Variant variant, AbstractUnitEntity unit, [CanBeNull] IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots, out string matchedBy)
	{
		if (variant == null)
		{
			matchedBy = null;
			return false;
		}
		return TryMatchTriggers(variant.AnyOfItems, variant.AnyOfFeatures, unit, slots, out matchedBy);
	}

	private static bool TryMatchTriggers([CanBeNull] BlueprintItemAugmentReference[] anyOfItems, [CanBeNull] BlueprintFeatureReference[] anyOfFeatures, AbstractUnitEntity unit, [CanBeNull] IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots, out string matchedBy)
	{
		if (anyOfItems != null)
		{
			for (int i = 0; i < anyOfItems.Length; i++)
			{
				BlueprintItemAugment blueprintItemAugment = anyOfItems[i]?.Get();
				if (blueprintItemAugment != null && IsAugmentEquipped(slots, blueprintItemAugment))
				{
					matchedBy = "item:" + blueprintItemAugment.name;
					return true;
				}
			}
		}
		if (anyOfFeatures != null)
		{
			for (int i = 0; i < anyOfFeatures.Length; i++)
			{
				BlueprintFeature blueprintFeature = anyOfFeatures[i]?.Get();
				if (blueprintFeature != null && unit.Facts.Get<Feature>(blueprintFeature) != null)
				{
					matchedBy = "feature:" + blueprintFeature.name;
					return true;
				}
			}
		}
		matchedBy = null;
		return false;
	}

	private static bool GroupReferencesItem(VariantGroup group, [CanBeNull] BlueprintItemAugment item)
	{
		if (item == null || group.Variants == null)
		{
			return false;
		}
		Variant[] variants = group.Variants;
		foreach (Variant variant in variants)
		{
			if (variant?.AnyOfItems == null)
			{
				continue;
			}
			BlueprintItemAugmentReference[] anyOfItems = variant.AnyOfItems;
			for (int j = 0; j < anyOfItems.Length; j++)
			{
				if (anyOfItems[j]?.Get() == item)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool GroupReferencesFeature(VariantGroup group, [CanBeNull] BlueprintFeature feature)
	{
		if (feature == null || group.Variants == null)
		{
			return false;
		}
		Variant[] variants = group.Variants;
		foreach (Variant variant in variants)
		{
			if (variant?.AnyOfFeatures == null)
			{
				continue;
			}
			BlueprintFeatureReference[] anyOfFeatures = variant.AnyOfFeatures;
			for (int j = 0; j < anyOfFeatures.Length; j++)
			{
				if (anyOfFeatures[j]?.Get() == feature)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void SetRenderersActive([CanBeNull] SkinnedMeshRenderer[] renderers, bool active)
	{
		if (renderers == null)
		{
			return;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
		{
			if (skinnedMeshRenderer != null)
			{
				skinnedMeshRenderer.gameObject.SetActive(active);
				skinnedMeshRenderer.enabled = active;
			}
		}
	}

	private static bool IsAugmentEquipped([CanBeNull] IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> slots, BlueprintItemAugment item)
	{
		if (slots == null || item == null)
		{
			return false;
		}
		foreach (KeyValuePair<BlueprintAugmentSlot, AugmentSlot> slot in slots)
		{
			if (slot.Value?.ItemBlueprint == item)
			{
				return true;
			}
		}
		return false;
	}

	private void SpawnWeapon(WeaponBinding binding, [CanBeNull] string matchedBy)
	{
		GameObject value;
		if (binding.WeaponPrefab == null || binding.AttachBone == null)
		{
			PFLog.TechArt.Log("[augmentManipulus] SpawnWeapon SKIP: binding='" + (binding.DebugName ?? "—") + "', prefab=" + ((binding.WeaponPrefab != null) ? binding.WeaponPrefab.name : "null") + ", bone=" + ((binding.AttachBone != null) ? binding.AttachBone.name : "null"));
		}
		else if (!_spawnedWeapons.TryGetValue(binding, out value) || !(value != null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(binding.WeaponPrefab, binding.AttachBone, worldPositionStays: false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			_spawnedWeapons[binding] = gameObject;
			PFLog.TechArt.Log("[augmentManipulus] SpawnWeapon: binding='" + (binding.DebugName ?? "—") + "', prefab=" + binding.WeaponPrefab.name + ", bone=" + binding.AttachBone.name + ", matchedBy='" + (matchedBy ?? "—") + "'");
		}
	}

	private void DestroySpawnedWeapon(WeaponBinding binding)
	{
		if (_spawnedWeapons.TryGetValue(binding, out var value))
		{
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
				PFLog.TechArt.Log("[augmentManipulus] DestroySpawnedWeapon: binding='" + (binding.DebugName ?? "—") + "'");
			}
			_spawnedWeapons.Remove(binding);
		}
	}

	private void OnValidate()
	{
		if (_variantGroups != null)
		{
			VariantGroup[] variantGroups = _variantGroups;
			foreach (VariantGroup variantGroup in variantGroups)
			{
				bool num = HasAny(variantGroup.DefaultRenderers);
				bool flag = HasAny(variantGroup.Variants);
				if (!num && !flag)
				{
					UnityEngine.Debug.LogWarning("[ManipulusAugmentRenderers] VariantGroup '" + variantGroup.DebugName + "' has neither DefaultRenderers nor Variants", this);
				}
				if (variantGroup.Variants == null)
				{
					continue;
				}
				Variant[] variants = variantGroup.Variants;
				foreach (Variant variant in variants)
				{
					if (variant != null)
					{
						if (!HasAny(variant.AnyOfItems) && !HasAny(variant.AnyOfFeatures))
						{
							UnityEngine.Debug.LogWarning("[ManipulusAugmentRenderers] Variant '" + variant.DebugName + "' in group '" + variantGroup.DebugName + "' has no triggers (AnyOfItems / AnyOfFeatures empty) — never matches", this);
						}
						if (!HasAny(variant.Renderers))
						{
							UnityEngine.Debug.LogWarning("[ManipulusAugmentRenderers] Variant '" + variant.DebugName + "' in group '" + variantGroup.DebugName + "' has no Renderers", this);
						}
					}
				}
			}
		}
		if (_weaponBindings == null)
		{
			return;
		}
		WeaponBinding[] weaponBindings = _weaponBindings;
		foreach (WeaponBinding weaponBinding in weaponBindings)
		{
			if (!HasAny(weaponBinding.AnyOfItems) && !HasAny(weaponBinding.AnyOfFeatures))
			{
				UnityEngine.Debug.LogWarning("[ManipulusAugmentRenderers] WeaponBinding '" + weaponBinding.DebugName + "' has no triggers (AnyOfItems / AnyOfFeatures empty) — never matches", this);
			}
			if (weaponBinding.WeaponPrefab == null || weaponBinding.AttachBone == null)
			{
				UnityEngine.Debug.LogWarning("[ManipulusAugmentRenderers] WeaponBinding '" + weaponBinding.DebugName + "' has incomplete data (WeaponPrefab / AttachBone)", this);
			}
		}
	}

	private static bool HasAny<T>([CanBeNull] T[] arr)
	{
		if (arr != null)
		{
			return arr.Length != 0;
		}
		return false;
	}
}
