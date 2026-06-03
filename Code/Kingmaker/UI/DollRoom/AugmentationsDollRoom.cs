using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.CharacterSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.DollRoom;

public class AugmentationsDollRoom : CharacterDollRoom
{
	[Serializable]
	public struct AreaBackgroundOverride
	{
		public BlueprintAreaReference Area;

		public Material BackgroundMaterial;
	}

	private const float m_RotationSpeed = 0.045f;

	private const string FemaleUnderwearName = "EE_Underwear1_F_Any";

	private const string MaleUnderwearName = "EE_Underwear1_M_Any";

	[FormerlySerializedAs("m_ShipPlaceholderDefaultRotationY")]
	[FormerlySerializedAs("m_BackgroundTintColor")]
	[Header("Additional")]
	[SerializeField]
	private float m_UnitPlaceholderDefaultRotationY;

	[SerializeField]
	private Vector3 m_defaultUnitPlaceholderPosition;

	[SerializeField]
	private Vector3 m_ManipuluisUnitPlaceholderPosition;

	[SerializeField]
	private GameObject m_UnitGameObject;

	[SerializeField]
	private GameObject m_LightObject;

	[Header("Background Overrides")]
	[SerializeField]
	private MeshRenderer m_BackgroundPlane;

	[SerializeField]
	private AreaBackgroundOverride[] m_AreaBackgroundOverrides;

	private Material m_DefaultBackgroundMaterial;

	private Quaternion m_UnitPlaceholderInitialRotationQuaternion;

	private float m_RotationOffcet = 0.5f;

	private float m_ManipulusLightOffset;

	protected override bool ShouldShowWeaponsInDollRoom => false;

	protected override bool ShouldShowConsumablesInDollRoom => false;

	public override void SetupUnit(BaseUnitEntity unit)
	{
		UnitViewHandsEquipment unitViewHandsEquipment = unit?.View?.HandsEquipment;
		if (unitViewHandsEquipment != null)
		{
			unitViewHandsEquipment.ShouldShowWeaponsInDollRoom = false;
			unitViewHandsEquipment.ShouldShowConsumablesInDollRoom = false;
			PFLog.TechArt.Log("[AugmentationsDollRoom.SetupUnit] Set flags on unit.View.HandsEquipment");
		}
		base.SetupUnit(unit);
		m_ManipulusLightOffset = m_ManipuluisUnitPlaceholderPosition.z - m_defaultUnitPlaceholderPosition.z;
		UpdateBackgroundForArea();
		ApplyAugmentationsCameraOffset(unit);
		SetUnitHeightForManipulus(unit);
		UpdateDoll(unit);
		SetUnitRotation();
		HideAllWeapons();
	}

	private void ApplyAugmentationsCameraOffset(BaseUnitEntity unit)
	{
		if (!m_Camera)
		{
			return;
		}
		Vector3 offset = Vector3.zero;
		DollRoomPositionOverride dollRoomPositionOverride = unit?.View?.GetComponent<DollRoomPositionOverride>();
		if (dollRoomPositionOverride != null && dollRoomPositionOverride.AugmentationsCameraOffset != Vector3.zero)
		{
			offset = dollRoomPositionOverride.AugmentationsCameraOffset;
		}
		else
		{
			DollRoomCameraZoomPreset dollRoomCameraZoomPreset = unit?.View?.CharacterAvatar?.Skeleton?.DollRoomZoomPreset;
			if (dollRoomCameraZoomPreset != null && dollRoomCameraZoomPreset.AugmentationsCameraOffset != Vector3.zero)
			{
				offset = dollRoomCameraZoomPreset.AugmentationsCameraOffset;
			}
		}
		m_Camera.ApplyCameraOffset(offset);
	}

	private void SetUnitHeightForManipulus(BaseUnitEntity unit)
	{
		bool flag = unit.Facts.Get(UIConfig.Instance.ManipulusOccupationReference.Get()) != null;
		m_UnitGameObject.transform.localPosition = (flag ? m_ManipuluisUnitPlaceholderPosition : m_defaultUnitPlaceholderPosition);
		m_LightObject.transform.localPosition = (flag ? (m_LightObject.transform.localPosition + new Vector3(0f, 0f, m_ManipulusLightOffset)) : Vector3.zero);
	}

	protected override void SetAvatar(Character avatar, bool activateAvatar = true)
	{
		if (m_AvatarHands != null)
		{
			m_AvatarHands.ShouldShowWeaponsInDollRoom = false;
			m_AvatarHands.ShouldShowConsumablesInDollRoom = false;
			PFLog.TechArt.Log("[AugmentationsDollRoom] SetAvatar: Set flags BEFORE base.SetAvatar()");
		}
		base.SetAvatar(avatar, activateAvatar);
		if (m_AvatarHands != null)
		{
			m_AvatarHands.HideConsumables();
			PFLog.TechArt.Log("[AugmentationsDollRoom] SetAvatar: Called HideConsumables()");
		}
	}

	public override void Hide()
	{
		UnitViewHandsEquipment unitViewHandsEquipment = base.Unit?.View?.HandsEquipment;
		if (unitViewHandsEquipment != null)
		{
			unitViewHandsEquipment.ShouldShowWeaponsInDollRoom = true;
		}
		base.Hide();
	}

	private void HideAllWeapons()
	{
		UnitViewHandsEquipment unitViewHandsEquipment = base.Unit?.View?.HandsEquipment;
		if (unitViewHandsEquipment != null)
		{
			WeaponSet selectedWeaponSet = unitViewHandsEquipment.GetSelectedWeaponSet();
			selectedWeaponSet.MainHand.AttachModel(toHand: false);
			selectedWeaponSet.OffHand.AttachModel(toHand: false);
			PFLog.TechArt.Log("[AugmentationsDollRoom] HideAllWeapons: Attached models to false");
		}
		if (m_Avatar != null && m_Avatar.AnimationManager != null)
		{
			m_Avatar.AnimationManager.ActiveMainHandWeaponStyle = WeaponAnimationStyle.Fist;
			m_Avatar.AnimationManager.ActiveOffHandWeaponStyle = WeaponAnimationStyle.Fist;
			PFLog.TechArt.Log("[AugmentationsDollRoom] HideAllWeapons: Set weapon style to Fist");
		}
	}

	protected override void UpdateInternal()
	{
		if (m_Avatar != null && m_Avatar.AnimationManager != null)
		{
			m_Avatar.AnimationManager.IsInCombat = false;
			m_Avatar.AnimationManager.ActiveMainHandWeaponStyle = WeaponAnimationStyle.Fist;
			m_Avatar.AnimationManager.ActiveOffHandWeaponStyle = WeaponAnimationStyle.Fist;
		}
		UnitViewHandsEquipment unitViewHandsEquipment = base.Unit?.View?.HandsEquipment;
		if (unitViewHandsEquipment != null)
		{
			WeaponSet selectedWeaponSet = unitViewHandsEquipment.GetSelectedWeaponSet();
			selectedWeaponSet.MainHand.AttachModel(toHand: false);
			selectedWeaponSet.OffHand.AttachModel(toHand: false);
		}
	}

	private void SetUnitRotation()
	{
		m_UnitPlaceholderInitialRotationQuaternion.eulerAngles = new Vector3(0f, m_UnitPlaceholderDefaultRotationY, 0f);
		m_TargetPlaceholder.transform.rotation = m_UnitPlaceholderInitialRotationQuaternion;
	}

	public void RotateToDefaultPosition()
	{
		if (m_TargetPlaceholder.rotation.eulerAngles.y >= m_UnitPlaceholderInitialRotationQuaternion.eulerAngles.y + m_RotationOffcet || m_TargetPlaceholder.rotation.eulerAngles.y <= m_UnitPlaceholderInitialRotationQuaternion.eulerAngles.y - m_RotationOffcet)
		{
			StopCoroutine(RotateBack());
			StartCoroutine(RotateBack());
		}
	}

	private IEnumerator RotateBack()
	{
		while (m_TargetPlaceholder.rotation.eulerAngles.y >= m_UnitPlaceholderInitialRotationQuaternion.eulerAngles.y + m_RotationOffcet || m_TargetPlaceholder.rotation.eulerAngles.y <= m_UnitPlaceholderInitialRotationQuaternion.eulerAngles.y - m_RotationOffcet)
		{
			m_TargetPlaceholder.rotation = Quaternion.Lerp(m_TargetPlaceholder.rotation, m_UnitPlaceholderInitialRotationQuaternion, 0.045f);
			yield return null;
		}
		EventBus.RaiseEvent(delegate(IAgumentationsDollRotationHandler h)
		{
			h.HandleOnRotationStop();
		});
	}

	protected override void OnAfterEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (base.Unit != null)
		{
			ApplyAugmentationsCameraOffset(base.Unit);
			UpdateDoll(base.Unit);
			HideAllWeapons();
			PFLog.TechArt.Log("[AugmentationsDollRoom] OnAfterEquipmentSlotUpdated: re-applied naked state");
		}
	}

	private EquipmentEntity GetTargetUnderwear(BaseUnitEntity unit, BlueprintCharGenRoot root)
	{
		EquipmentEntityLink[] obj = ((unit.Gender == Gender.Female) ? root.FemaleClothes : root.MaleClothes);
		string targetName = ((unit.Gender == Gender.Female) ? "EE_Underwear1_F_Any" : "EE_Underwear1_M_Any");
		return obj?.Select((EquipmentEntityLink link) => link.Load()).FirstOrDefault((EquipmentEntity ee) => ee != null && ee.name == targetName);
	}

	private static bool IsUnderwear(EquipmentEntity ee)
	{
		return (ee?.name?.ToLower().Contains("underwear")).GetValueOrDefault();
	}

	private void UpdateDoll(BaseUnitEntity unit)
	{
		bool flag = unit != null && unit.Body?.Mechadendrites?.Any((EquipmentSlot<BlueprintItemMechadendrite> slot) => slot?.MaybeItem != null) == true;
		if (!unit.IsMainCharacter && flag)
		{
			PFLog.TechArt.Log("[AugmentationsDollRoom] Skip unclothe for companion with mechadendrites: " + unit.CharacterName);
			return;
		}
		Character avatar = m_Avatar;
		BlueprintCharGenRoot instance = BlueprintCharGenRoot.Instance;
		if (avatar == null)
		{
			PFLog.TechArt.Log("[AugmentationsDollRoom] UpdateDoll skipped for " + unit.CharacterName + ": SimpleAvatar path (m_Avatar == null)");
			return;
		}
		PFLog.TechArt.Log($"[AugmentationsDollRoom] UpdateDoll started for unit: {unit.CharacterName}, Gender: {unit.Gender}, IsMainCharacter: {unit.IsMainCharacter}");
		avatar.SavedBeforeCutsceneRampIndices.Clear();
		foreach (Character.SelectedRampIndices rampIndex in avatar.RampIndices)
		{
			avatar.SavedBeforeCutsceneRampIndices.Add(rampIndex);
		}
		List<EquipmentEntity> list;
		if (unit.IsMainCharacter)
		{
			DollData doll = unit.ViewSettings.Doll;
			if (doll != null && doll.EquipmentEntityIds.Count > 0)
			{
				list = (from id in doll.EquipmentEntityIds
					select ResourcesLibrary.TryGetResource<EquipmentEntity>(id) into ee
					where ee != null
					select ee).ToList();
				if (doll.RacePreset != null)
				{
					IEnumerable<EquipmentEntity> collection = from link in doll.RacePreset.Skin.GetLinks(unit.Gender, doll.RacePreset.RaceId)
						select link.Load() into ee
						where ee != null
						select ee;
					list.AddRange(collection);
				}
				PFLog.TechArt.Log($"[AugmentationsDollRoom] RT mode (DollData): {list.Count} EE (with Skin)");
			}
			else
			{
				List<EquipmentEntity> dontUnequipLoaded = (from x in ((unit.Gender == Gender.Male) ? instance.MaleDontUnequip : instance.FemaleDontUnequip)?.Select((EquipmentEntityLink x) => x.Load())
					where x != null
					select x).ToList() ?? new List<EquipmentEntity>();
				list = avatar.EquipmentEntities.Where((EquipmentEntity ee) => dontUnequipLoaded.Contains(ee)).ToList();
				PFLog.TechArt.Log($"[AugmentationsDollRoom] RT mode (Fallback): Original EE={avatar.EquipmentEntities.Count}, DontUnequip={dontUnequipLoaded.Count}, Kept={list.Count}");
			}
		}
		else
		{
			DollData doll2 = unit.ViewSettings.Doll;
			if (doll2 != null && doll2.EquipmentEntityIds.Count > 0)
			{
				list = (from id in doll2.EquipmentEntityIds
					select ResourcesLibrary.TryGetResource<EquipmentEntity>(id) into ee
					where ee != null
					select ee).ToList();
				if (doll2.RacePreset != null)
				{
					IEnumerable<EquipmentEntity> collection2 = from link in doll2.RacePreset.Skin.GetLinks(unit.Gender, doll2.RacePreset.RaceId)
						select link.Load() into ee
						where ee != null
						select ee;
					list.AddRange(collection2);
				}
				PFLog.TechArt.Log($"[AugmentationsDollRoom] Companion/Hireling mode (DollData): {list.Count} EE (with Skin)");
			}
			else
			{
				list = (from link in m_OriginalAvatar.SavedEquipmentEntities
					select link.Load() into ee
					where ee != null
					where !ee.name.ToLower().Contains("baseoutfit")
					where !ee.name.ToLower().Contains("baseou")
					where !ee.name.ToLower().Contains("cape")
					where !ee.name.ToLower().Contains("backpack")
					where !ee.name.ToLower().Contains("armor")
					where !ee.name.ToLower().Contains("gloves")
					where !ee.name.ToLower().Contains("belt")
					where !ee.name.ToLower().Contains("pants")
					where !ee.name.ToLower().Contains("boots")
					where !ee.name.ToLower().Contains("helmet")
					select ee).ToList();
				PFLog.TechArt.Log($"[AugmentationsDollRoom] Companion mode (SavedEquipmentEntities): {avatar.SavedEquipmentEntities.Count} -> {list.Count} after filtering");
			}
		}
		avatar.RemoveAllEquipmentEntities();
		foreach (EquipmentEntity item in list)
		{
			if (!IsUnderwear(item))
			{
				AddEquipmentWithRamps(item);
			}
		}
		EquipmentEntity targetUnderwear = GetTargetUnderwear(unit, instance);
		if (targetUnderwear != null)
		{
			AddEquipmentWithRamps(targetUnderwear);
			PFLog.TechArt.Log("[AugmentationsDollRoom] Added underwear: " + targetUnderwear.name);
		}
		else
		{
			PFLog.TechArt.Log($"[AugmentationsDollRoom] Target underwear not found in CharGenRoot clothes for gender {unit.Gender}");
		}
		IReadOnlyDictionary<BlueprintAugmentSlot, AugmentSlot> readOnlyDictionary = unit.Body?.Augments?.Slots;
		if (readOnlyDictionary != null)
		{
			Gender gender = unit.Gender;
			Race valueOrDefault = (unit.Progression?.Race?.RaceId).GetValueOrDefault();
			foreach (KeyValuePair<BlueprintAugmentSlot, AugmentSlot> item2 in readOnlyDictionary)
			{
				AugmentSlot value = item2.Value;
				if (value?.MaybeItem == null)
				{
					continue;
				}
				BlueprintItemEquipment blueprintItemEquipment = value.MaybeItem.Blueprint as BlueprintItemEquipment;
				if (blueprintItemEquipment?.EquipmentEntity == null)
				{
					continue;
				}
				foreach (EquipmentEntity item3 in blueprintItemEquipment.EquipmentEntity.Load(gender, valueOrDefault))
				{
					AddEquipmentWithRamps(item3);
				}
				PFLog.TechArt.Log("[AugmentationsDollRoom] Added augmentation EEs from slot " + item2.Key?.name);
			}
		}
		DollState dollState = new DollState();
		dollState.SetupFromUnit(unit);
		Skeleton skeleton = dollState.GetSkeleton();
		if (skeleton != null && avatar.Skeleton != skeleton)
		{
			avatar.Skeleton = skeleton;
		}
		dollState.ApplyRamps(avatar);
		avatar.gameObject.SetActive(value: true);
		PFLog.TechArt.Log($"[AugmentationsDollRoom] UpdateDoll completed. Avatar has {avatar.EquipmentEntities.Count} EquipmentEntities");
		void AddEquipmentWithRamps(EquipmentEntity ee)
		{
			if (!(ee == null) && !avatar.EquipmentEntities.Contains(ee))
			{
				avatar.AddEquipmentEntity(ee);
				Character.SelectedRampIndices selectedRampIndices = avatar.SavedBeforeCutsceneRampIndices.FirstOrDefault((Character.SelectedRampIndices r) => r.EquipmentEntity == ee);
				if (selectedRampIndices != null)
				{
					avatar.SetRampIndices(ee, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
				}
			}
		}
	}

	private void UpdateBackgroundForArea()
	{
		if (m_BackgroundPlane == null || m_AreaBackgroundOverrides == null)
		{
			return;
		}
		if ((object)m_DefaultBackgroundMaterial == null)
		{
			m_DefaultBackgroundMaterial = m_BackgroundPlane.sharedMaterial;
		}
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		AreaBackgroundOverride[] areaBackgroundOverrides = m_AreaBackgroundOverrides;
		for (int i = 0; i < areaBackgroundOverrides.Length; i++)
		{
			AreaBackgroundOverride areaBackgroundOverride = areaBackgroundOverrides[i];
			if (areaBackgroundOverride.Area?.Get() == currentlyLoadedArea)
			{
				m_BackgroundPlane.sharedMaterial = areaBackgroundOverride.BackgroundMaterial;
				return;
			}
		}
		m_BackgroundPlane.sharedMaterial = m_DefaultBackgroundMaterial;
	}
}
