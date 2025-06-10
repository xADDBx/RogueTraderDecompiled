using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class UnitViewHandSlotData
{
	private readonly struct NotifyOwnerRenderersChangedScope : IDisposable
	{
		private readonly UnitViewHandSlotData m_Data;

		private readonly GameObject m_InitialVisualModel;

		private readonly GameObject m_InitialSheathVisualModel;

		public NotifyOwnerRenderersChangedScope(UnitViewHandSlotData data)
		{
			m_Data = data;
			m_InitialVisualModel = m_Data.VisualModel;
			m_InitialSheathVisualModel = m_Data.SheathVisualModel;
		}

		public void Dispose()
		{
			if (m_InitialVisualModel != m_Data.VisualModel || m_InitialSheathVisualModel != m_Data.SheathVisualModel)
			{
				m_Data.m_Equipment.View.MarkRenderersAndCollidersAreUpdated();
			}
		}
	}

	private readonly UnitViewHandsEquipment m_Equipment;

	private readonly bool m_IsMainHand;

	private readonly int m_SlotIdx;

	private readonly List<Renderer> m_VisualModelRenderers = new List<Renderer>();

	private readonly List<Renderer> m_SheathModelRenderers = new List<Renderer>();

	private Transform m_MainHandTransform;

	private Transform m_OffHandTransform;

	private UnitEquipmentVisualSlotType m_VisualSlot;

	private UnitEquipmentAnimationSlotType m_EquipmentSlotType;

	public HandSlot Slot
	{
		get
		{
			if (!m_IsMainHand)
			{
				return Owner.Body.HandsEquipmentSets[m_SlotIdx].SecondaryHand;
			}
			return Owner.Body.HandsEquipmentSets[m_SlotIdx].PrimaryHand;
		}
	}

	public GameObject VisualModel { get; private set; }

	public GameObject SheathVisualModel { get; private set; }

	public ItemEntity VisibleItem { get; private set; }

	public bool IsInHand { get; set; }

	public BaseUnitEntity Owner => m_Equipment.Owner;

	public Character Character => m_Equipment.Character;

	public Transform MainHandTransform
	{
		get
		{
			if (Slot.MaybeWeapon != null && Owner.GetOptional<UnitPartMechadendrites>() != null && !Slot.MaybeWeapon.Blueprint.IsMelee)
			{
				m_MainHandTransform = Character.transform.FindChildRecursive("R_MechadendriteWeaponBone");
			}
			else
			{
				m_MainHandTransform = Character.transform.FindChildRecursive("R_WeaponBone");
			}
			if (m_MainHandTransform != null)
			{
				return m_MainHandTransform;
			}
			m_MainHandTransform = Character.transform.FindChildRecursive("R_Hand_ADJ");
			return m_MainHandTransform;
		}
	}

	public Transform OffHandTransform
	{
		get
		{
			if (Slot.MaybeWeapon != null && Owner.GetOptional<UnitPartMechadendrites>() != null && !Slot.MaybeWeapon.Blueprint.IsMelee)
			{
				m_OffHandTransform = Character.transform.FindChildRecursive("R_MechadendriteWeaponBone");
			}
			else if (Slot.MaybeWeapon != null && Owner.GetOptional<UnitPartMechadendrites>() != null)
			{
				m_OffHandTransform = Character.transform.FindChildRecursive("R_WeaponBone");
			}
			else
			{
				m_OffHandTransform = Character.transform.FindChildRecursive("L_WeaponBone");
			}
			if (m_OffHandTransform != null)
			{
				return m_OffHandTransform;
			}
			m_OffHandTransform = Character.transform.FindChildRecursive("L_Hand_ADJ");
			return m_OffHandTransform;
		}
	}

	public Transform HandTransform
	{
		get
		{
			if (!m_IsMainHand)
			{
				return OffHandTransform;
			}
			return MainHandTransform;
		}
	}

	public bool IsOff => !m_IsMainHand;

	private BlueprintItemEquipmentHand VisibleItemBlueprint => (BlueprintItemEquipmentHand)(VisibleItem?.Blueprint);

	public bool IsMismatched => VisibleItem != Slot.MaybeItem;

	public UnitEquipmentAnimationSlotType EquipmentSlotType => VisualSlot.GetAnimSlot();

	public bool CanAnimate => EquipmentSlotType != UnitEquipmentAnimationSlotType.None;

	public UnitEquipmentVisualSlotType VisualSlot
	{
		get
		{
			return m_VisualSlot;
		}
		set
		{
			m_VisualSlot = value;
		}
	}

	public UnitViewHandSlotData Other
	{
		get
		{
			if (!m_IsMainHand)
			{
				return m_Equipment.Sets[Slot.HandsEquipmentSet].MainHand;
			}
			return m_Equipment.Sets[Slot.HandsEquipmentSet].OffHand;
		}
	}

	private float OwnerWeaponScale
	{
		get
		{
			float num = Owner.View.GetSizeScale();
			if (Owner.View.DoNotAdjustScale)
			{
				num = Owner.View.ViewTransform.localScale.x / Owner.View.OriginalScale.x;
			}
			EquipmentOffsets component = VisualModel.GetComponent<EquipmentOffsets>();
			if (component != null && component.raceScaleList.Count > 0)
			{
				BaseUnitEntity owner = Owner;
				if (owner != null)
				{
					PartUnitProgression progression = owner.Progression;
					if (progression != null)
					{
						BlueprintRace race = progression.Race;
						if (race != null)
						{
							_ = race.RaceId;
							if (true)
							{
								foreach (EquipmentOffsets.RaceScale raceScale in component.raceScaleList)
								{
									if (Owner.Progression.Race.RaceId == raceScale.race)
									{
										num *= raceScale.WeaponScale;
										break;
									}
								}
							}
						}
					}
				}
			}
			return num;
		}
	}

	public bool IsActiveSet => Slot.HandsEquipmentSet == Owner.Body.CurrentHandsEquipmentSet;

	public UnitViewHandSlotData(UnitViewHandsEquipment equipment, int slotIdx, bool isMainHand)
	{
		m_IsMainHand = isMainHand;
		m_Equipment = equipment;
		m_SlotIdx = slotIdx;
	}

	private void RecreateModel()
	{
		DestroyModelIfExists();
		GameObject fxPrefab = GetFxPrefab();
		if ((bool)fxPrefab)
		{
			VisualModel = UnityEngine.Object.Instantiate(fxPrefab);
			VisualModel.transform.localScale *= OwnerWeaponScale;
			WeaponParticlesSnapMap component = VisualModel.GetComponent<WeaponParticlesSnapMap>();
			if ((bool)component)
			{
				component.ParticleSizeScale *= OwnerWeaponScale;
			}
		}
		DestroySheathModelIfExists();
		if ((bool)VisibleItemBlueprint && (bool)VisibleItemBlueprint.VisualParameters.SheathModel)
		{
			ReattachSheath();
		}
	}

	private GameObject GetFxPrefab()
	{
		GameObject result = null;
		if ((bool)VisibleItemBlueprint)
		{
			result = VisibleItemBlueprint.VisualParameters.Model;
			if (!IsInHand && (bool)VisibleItemBlueprint.VisualParameters.BeltModel)
			{
				result = VisibleItemBlueprint.VisualParameters.BeltModel;
			}
		}
		return result;
	}

	private void ReattachSheath()
	{
		Transform transform = null;
		if (VisibleItemBlueprint.VisualParameters.HasQuiver)
		{
			UnitViewHandSlotData unitViewHandSlotData = m_Equipment.QuiverHandSlot;
			if (unitViewHandSlotData == this)
			{
				unitViewHandSlotData = null;
			}
			if (unitViewHandSlotData != null && (IsActiveSet || unitViewHandSlotData.SheathVisualModel == null || unitViewHandSlotData.VisibleItemBlueprint == null || !unitViewHandSlotData.VisibleItemBlueprint.VisualParameters.HasQuiver))
			{
				unitViewHandSlotData.DestroySheathModelIfExists();
			}
		}
		else
		{
			if (m_Equipment.QuiverHandSlot == this)
			{
				m_Equipment.QuiverHandSlot = null;
			}
			transform = m_Equipment.GetVisualSlotBone(VisualSlot);
		}
		if ((bool)transform)
		{
			if (!SheathVisualModel)
			{
				GameObject sheathModel = VisibleItemBlueprint.VisualParameters.SheathModel;
				SheathVisualModel = UnityEngine.Object.Instantiate(sheathModel);
				SheathVisualModel.transform.localPosition = Vector3.zero;
				SheathVisualModel.transform.localRotation = Quaternion.identity;
				SheathVisualModel.transform.localScale *= OwnerWeaponScale;
			}
			SheathVisualModel.transform.parent = transform;
			EquipmentOffsets component = SheathVisualModel.GetComponent<EquipmentOffsets>();
			if (!component && (bool)VisibleItemBlueprint.VisualParameters.Model)
			{
				component = VisibleItemBlueprint.VisualParameters.Model.GetComponent<EquipmentOffsets>();
			}
			if ((bool)component)
			{
				component.Apply(VisualSlot, IsOff, Character, SheathVisualModel.transform);
			}
			SheathVisualModel.GetComponentsInChildren(m_SheathModelRenderers);
		}
	}

	private void DestroySheathModelIfExists()
	{
		if (!(SheathVisualModel == null))
		{
			UnityEngine.Object.Destroy(SheathVisualModel);
			SheathVisualModel = null;
			m_SheathModelRenderers.Clear();
		}
	}

	public void AttachModel(bool toHand)
	{
		IsInHand = toHand || (IsOff && Owner.GetOptional<UnitPartMechadendrites>() != null);
		AttachModel();
	}

	public void AttachModel()
	{
		if (!VisualModel)
		{
			return;
		}
		using (new NotifyOwnerRenderersChangedScope(this))
		{
			IsInHand &= !Slot.Disabled;
			Transform transform;
			if (IsInHand)
			{
				transform = ((VisibleItemBlueprint.VisualParameters.IsBow && !IsOff) ? OffHandTransform : HandTransform);
				VisualModel.SetActive(value: true);
			}
			else
			{
				if (((VisualSlot == UnitEquipmentVisualSlotType.RightBack01 || VisualSlot == UnitEquipmentVisualSlotType.LeftBack01) && Character.EquipmentEntities.SelectMany((EquipmentEntity ee) => ee.OutfitParts).Any((EquipmentEntity.OutfitPart p) => p.Special == EquipmentEntity.OutfitPartSpecialType.Backpack)) || Owner.GetOptional<UnitPartMechadendrites>() != null)
				{
					VisualModel.SetActive(value: false);
				}
				else if (!(VisibleItem is ItemEntityShield) && m_Equipment.Sets.Count((KeyValuePair<HandsEquipmentSet, WeaponSet> x) => x.Value.MainHand.VisibleItem == VisibleItem || x.Value.OffHand.VisibleItem == VisibleItem) > 1)
				{
					VisualModel.SetActive(value: false);
				}
				transform = m_Equipment.GetVisualSlotBone(VisualSlot);
			}
			if (!transform)
			{
				DestroyModelIfExists();
				DestroySheathModelIfExists();
				return;
			}
			if ((bool)VisibleItemBlueprint.VisualParameters.BeltModel)
			{
				RecreateModel();
			}
			else if ((bool)VisibleItemBlueprint.VisualParameters.SheathModel)
			{
				ReattachSheath();
			}
			VisualModel.transform.SetParent(transform, worldPositionStays: false);
			VisualModel.transform.localPosition = Vector3.zero;
			VisualModel.transform.localRotation = Quaternion.identity;
			VisualModel.GetComponentsInChildren(m_VisualModelRenderers);
			EquipmentOffsets component = VisualModel.GetComponent<EquipmentOffsets>();
			IkRaceOffsetApply(component);
			if ((bool)component)
			{
				if (VisibleItemBlueprint is BlueprintItemWeapon { IsMelee: false } && Owner.GetOptional<UnitPartMechadendrites>() != null)
				{
					UnitPartMechadendrites optional = Owner.GetOptional<UnitPartMechadendrites>();
					if (optional != null && optional.Mechadendrites.ContainsKey(MechadendritesType.Ballistic))
					{
						goto IL_02ac;
					}
				}
				component.Apply((!IsInHand) ? VisualSlot : UnitEquipmentVisualSlotType.None, IsOff || VisibleItemBlueprint.VisualParameters.IsBow, Character);
				if ((bool)component.JointsParent)
				{
					component.JointsParent.SetParent(Character.transform);
					RigidbodyWeaponParentUpperInDollroom(component);
					MirrorRigidbodyWeaponForMirroredCharacter(component);
				}
			}
			goto IL_02ac;
			IL_02ac:
			ShowItem(Owner.View.IsVisible);
			m_Equipment.RaiseModelSpawned();
			EventBus.RaiseEvent(delegate(IVisualWeaponStateChangeHandle h)
			{
				h.VisualWeaponStateChangeHandle((!IsInHand) ? VFXSpeedUpdater.WeaponVisualState.OutHand : VFXSpeedUpdater.WeaponVisualState.InHand, VisualModel);
			});
		}
	}

	private void RigidbodyWeaponParentUpperInDollroom(EquipmentOffsets offset)
	{
		CharacterDollRoom componentInParent = offset.GetComponentInParent<CharacterDollRoom>();
		if (componentInParent != null)
		{
			offset.JointsParent.SetParent(componentInParent.transform);
		}
	}

	private void MirrorRigidbodyWeaponForMirroredCharacter(EquipmentOffsets offset)
	{
		if (Owner.View.CharacterAvatar != null || offset.JointsParent.GetComponentInChildren<CharacterJoint>() == null || offset.transform.Find("MirrorRigidbodyWeaponForMirroredCharacter") != null)
		{
			return;
		}
		offset.JointsParent.localScale = new Vector3(Math.Abs(offset.JointsParent.localScale.x), 0f - Math.Abs(offset.JointsParent.localScale.y), 0f - Math.Abs(offset.JointsParent.localScale.z));
		List<Transform> list = new List<Transform>();
		foreach (Transform item in offset.transform)
		{
			list.Add(item);
		}
		GameObject gameObject = new GameObject("MirrorRigidbodyWeaponForMirroredCharacter");
		gameObject.transform.parent = offset.transform;
		gameObject.transform.ResetAll();
		foreach (Transform item2 in list)
		{
			item2.transform.parent = gameObject.transform;
		}
		gameObject.transform.localScale = new Vector3(-1f, -1f, -1f);
		gameObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		CharacterJoint[] componentsInChildren = offset.JointsParent.GetComponentsInChildren<CharacterJoint>();
		foreach (CharacterJoint obj in componentsInChildren)
		{
			obj.anchor = new Vector3(1f, 0f, 0f);
			obj.anchor = new Vector3(0f, 0f, 0f);
		}
	}

	private void IkRaceOffsetApply(EquipmentOffsets offset)
	{
		if (!(offset != null) || offset.raceOffset.Count <= 0)
		{
			return;
		}
		BaseUnitEntity owner = Owner;
		if (owner == null)
		{
			return;
		}
		PartUnitProgression progression = owner.Progression;
		if (progression == null)
		{
			return;
		}
		BlueprintRace race = progression.Race;
		if (race == null)
		{
			return;
		}
		_ = race.RaceId;
		if (1 == 0)
		{
			return;
		}
		foreach (EquipmentOffsets.RaceOffset item in offset.raceOffset)
		{
			if (Owner.Progression.Race.RaceId == item.race)
			{
				offset.IkTargetLeftHand.localPosition += item.OffsetLeftIk;
				item.OffsetLeftIk = new Vector3(0f, 0f, 0f);
				break;
			}
		}
	}

	private void UpdateWeaponEnchantmentFx(bool isVisible)
	{
		if (!m_Equipment.IsDollRoom && UIDollRooms.Instance != null && UIDollRooms.Instance.CharacterDollRoom != null && UIDollRooms.Instance.CharacterDollRoom.Unit == Owner)
		{
			return;
		}
		if (IsInHand && isVisible)
		{
			WeaponParticlesSnapMap weaponParticlesSnapMap = ((VisualModel != null) ? VisualModel.GetComponent<WeaponParticlesSnapMap>() : null);
			bool flag = weaponParticlesSnapMap != Slot.FxSnapMap;
			Slot.FxSnapMap = weaponParticlesSnapMap;
			{
				foreach (ItemEnchantment enchantment in VisibleItem.Enchantments)
				{
					if (!enchantment.FxObject || flag)
					{
						enchantment.RespawnFx();
					}
				}
				return;
			}
		}
		foreach (ItemEnchantment enchantment2 in VisibleItem.Enchantments)
		{
			enchantment2.DestroyFx();
		}
	}

	public void MatchVisuals()
	{
		using (new NotifyOwnerRenderersChangedScope(this))
		{
			if (VisibleItem?.UniqueId != Slot.MaybeItem?.UniqueId)
			{
				m_VisualSlot = UnitEquipmentVisualSlotType.None;
				DestroyModelIfExists();
				VisibleItem = Slot.MaybeItem;
			}
			else
			{
				RecreateModel();
				AttachModel();
				m_Equipment.RaiseModelSpawned();
			}
		}
	}

	public void ShowItem(bool isVisible)
	{
		if (m_Equipment.IsDollRoom)
		{
			isVisible = true;
		}
		if ((bool)VisualModel)
		{
			foreach (Renderer visualModelRenderer in m_VisualModelRenderers)
			{
				if (visualModelRenderer != null)
				{
					if (!visualModelRenderer.CompareTag("NoValidate"))
					{
						visualModelRenderer.enabled = isVisible;
					}
					else
					{
						visualModelRenderer.enabled = false;
					}
				}
			}
			if (!m_Equipment.IsUsingHologram)
			{
				UpdateWeaponEnchantmentFx(isVisible);
			}
		}
		if (!SheathVisualModel)
		{
			return;
		}
		foreach (Renderer sheathModelRenderer in m_SheathModelRenderers)
		{
			sheathModelRenderer.enabled = isVisible;
		}
	}

	public void GetPossibleVisualSlots(List<UnitEquipmentVisualSlotType> possibleSlots)
	{
		UnitEquipmentVisualSlotType[] attachSlots = VisibleItemBlueprint.VisualParameters.AttachSlots;
		foreach (UnitEquipmentVisualSlotType unitEquipmentVisualSlotType in attachSlots)
		{
			if ((VisibleItemBlueprint.VisualParameters.IsTwoHanded || !IsOff || !unitEquipmentVisualSlotType.IsLeft()) && (VisibleItemBlueprint.VisualParameters.IsTwoHanded || IsOff || !unitEquipmentVisualSlotType.IsRight()) && (!VisibleItemBlueprint.VisualParameters.IsBow || !unitEquipmentVisualSlotType.IsLeft()))
			{
				possibleSlots.Add(unitEquipmentVisualSlotType);
			}
		}
	}

	public HandEquipmentHelper Equip(UnitEquipmentVisualSlotType equipFrom = UnitEquipmentVisualSlotType.None)
	{
		BlueprintItemEquipmentHand blueprintItemEquipmentHand = Slot.MaybeItem?.Blueprint as BlueprintItemEquipmentHand;
		bool flag = IsOff || (blueprintItemEquipmentHand?.VisualParameters.IsBow ?? false);
		UnitEquipmentAnimationSlotType unitEquipmentAnimationSlotType = ((equipFrom == UnitEquipmentVisualSlotType.None && EquipmentSlotType != 0) ? EquipmentSlotType : equipFrom.GetAnimSlot());
		if (unitEquipmentAnimationSlotType == UnitEquipmentAnimationSlotType.None && blueprintItemEquipmentHand != null)
		{
			UnitEquipmentVisualSlotType slot = blueprintItemEquipmentHand.VisualParameters.AttachSlots.FirstOrDefault();
			unitEquipmentAnimationSlotType = slot.GetAnimSlot();
			if (IsOff && slot.IsLeft())
			{
				unitEquipmentAnimationSlotType = blueprintItemEquipmentHand.VisualParameters.AttachSlots.FirstOrDefault((UnitEquipmentVisualSlotType s) => !s.IsLeft()).GetAnimSlot();
			}
		}
		if (unitEquipmentAnimationSlotType == UnitEquipmentAnimationSlotType.None)
		{
			AttachModel(toHand: true);
			return null;
		}
		WeaponAnimationStyle weaponStyleForHand = m_Equipment.GetWeaponStyleForHand(Slot);
		HandEquipmentHelper handEquipmentHelper = (flag ? HandEquipmentHelper.StartEquipOffHand(m_Equipment.AnimationManager, unitEquipmentAnimationSlotType, weaponStyleForHand) : HandEquipmentHelper.StartEquipMainHand(m_Equipment.AnimationManager, unitEquipmentAnimationSlotType, weaponStyleForHand));
		handEquipmentHelper.Callback = delegate
		{
			AttachModel(toHand: true);
		};
		m_Equipment.HandAnimations.Add(handEquipmentHelper);
		return handEquipmentHelper;
	}

	public HandEquipmentHelper Unequip()
	{
		if (VisibleItem == null)
		{
			return null;
		}
		if (EquipmentSlotType == UnitEquipmentAnimationSlotType.None)
		{
			AttachModel(toHand: false);
			return null;
		}
		bool num = IsOff || ((bool)VisibleItemBlueprint && VisibleItemBlueprint.VisualParameters.IsBow);
		ItemEntity visibleItem = VisibleItem;
		WeaponAnimationStyle weaponAnimationStyle = ((visibleItem is ItemEntityWeapon weapon) ? m_Equipment.GetWeaponStyleForWeapon(weapon) : ((visibleItem is ItemEntityShield) ? WeaponAnimationStyle.Shield : WeaponAnimationStyle.None));
		WeaponAnimationStyle style = weaponAnimationStyle;
		HandEquipmentHelper handEquipmentHelper = (num ? HandEquipmentHelper.StartUnequipOffHand(m_Equipment.AnimationManager, EquipmentSlotType, style) : HandEquipmentHelper.StartUnequipMainHand(m_Equipment.AnimationManager, EquipmentSlotType, style));
		handEquipmentHelper.Callback = delegate
		{
			AttachModel(toHand: false);
		};
		m_Equipment.HandAnimations.Add(handEquipmentHelper);
		return handEquipmentHelper;
	}

	public override string ToString()
	{
		int num = Owner.Body.HandsEquipmentSets.IndexOf(Slot.HandsEquipmentSet);
		return (IsOff ? "Off" : "Main") + "Hand/" + num;
	}

	public void DestroyModel()
	{
		using (new NotifyOwnerRenderersChangedScope(this))
		{
			DestroyModelIfExists();
		}
	}

	private void DestroyModelIfExists()
	{
		if (VisualModel == null)
		{
			return;
		}
		EquipmentOffsets component = VisualModel.GetComponent<EquipmentOffsets>();
		if ((bool)component && (bool)component.JointsParent)
		{
			UnityEngine.Object.Destroy(component.JointsParent.gameObject);
		}
		UnityEngine.Object.Destroy(VisualModel);
		VisualModel = null;
		m_VisualModelRenderers.Clear();
		if (VisibleItem == null)
		{
			return;
		}
		foreach (ItemEnchantment enchantment in VisibleItem.Enchantments)
		{
			enchantment.DestroyFx();
		}
	}
}
