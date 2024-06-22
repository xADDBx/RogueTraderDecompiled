using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.View.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class UnitViewHandsEquipment
{
	public readonly UnitEntityView View;

	public readonly Character Character;

	public readonly CountingGuard AreHandsBusyWithAnimation = new CountingGuard();

	private CoroutineHandler m_Coroutine;

	public readonly List<HandEquipmentHelper> HandAnimations = new List<HandEquipmentHelper>();

	private readonly bool m_IsActive;

	private WeaponSet m_ActiveSet;

	private bool m_IsAnimatingWeaponsChange;

	private readonly CountingGuard m_ShouldBeInCombat = new CountingGuard();

	private readonly CountingGuard m_ShouldBeInCombatVisual = new CountingGuard();

	public readonly Dictionary<HandsEquipmentSet, WeaponSet> Sets = new Dictionary<HandsEquipmentSet, WeaponSet>();

	private readonly Transform[] m_BonesByVisualSlot = new Transform[UnitEquipmentVisualSlotExtension.BoneNames.Length];

	private readonly UnitViewHandSlotData[] m_SlotsByVisualSlot = new UnitViewHandSlotData[UnitEquipmentVisualSlotExtension.BoneNames.Length];

	private readonly GameObject[] m_ConsumableSlots = new GameObject[4];

	private readonly List<Light> m_EquipmentLights = new List<Light>(2);

	private static readonly UnitEquipmentVisualSlotType[] s_ConsumableVisualSlots = new UnitEquipmentVisualSlotType[2]
	{
		UnitEquipmentVisualSlotType.RightFront01,
		UnitEquipmentVisualSlotType.LeftFront01
	};

	private GameObject m_PrevCloakObj;

	private LocatorPositionTracker m_MainHandTracker;

	private LocatorPositionTracker m_OffHandTracker;

	public bool IsUsingHologram { get; set; }

	public BaseUnitEntity Owner => View.EntityData;

	public UnitAnimationManager AnimationManager => Character.AnimationManager;

	public bool IsCombatStateConsistent => InCombat == ShouldBeInCombat;

	private bool ShouldBeInCombat
	{
		get
		{
			if (!m_ShouldBeInCombat)
			{
				return m_ShouldBeInCombatVisual;
			}
			return true;
		}
	}

	public WeaponAnimationStyle ActiveMainHandWeaponStyle => m_ActiveSet?.MainHand?.Slot?.GetWeaponStyle(IsDollRoom) ?? WeaponAnimationStyle.Fist;

	public WeaponAnimationStyle ActiveOffHandWeaponStyle => m_ActiveSet?.OffHand?.Slot?.GetWeaponStyle(IsDollRoom) ?? WeaponAnimationStyle.Fist;

	public bool IsDollRoom => View.HandsEquipment != this;

	public bool IsMainHandMismatched
	{
		get
		{
			if (m_ActiveSet.MainHand.VisibleItem == Owner.Body.PrimaryHand.MaybeItem)
			{
				if (InCombat && m_ActiveSet.MainHand.Slot.HasItem && !m_ActiveSet.MainHand.IsInHand)
				{
					return m_ActiveSet.MainHand.Slot.Active;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsOffHandMismatched
	{
		get
		{
			if (m_ActiveSet.OffHand.VisibleItem == Owner.Body.SecondaryHand.MaybeItem)
			{
				if (InCombat && m_ActiveSet.OffHand.Slot.HasItem && !m_ActiveSet.OffHand.IsInHand)
				{
					return m_ActiveSet.OffHand.Slot.Active;
				}
				return false;
			}
			return true;
		}
	}

	public bool InCombat { get; private set; }

	public UnitViewHandSlotData QuiverHandSlot { get; set; }

	public bool HasQuiverSpawned
	{
		get
		{
			if (QuiverHandSlot != null)
			{
				return QuiverHandSlot.SheathVisualModel;
			}
			return false;
		}
	}

	private bool Active
	{
		get
		{
			if (Owner?.Body != null && m_IsActive && !Owner.Body.IsPolymorphed)
			{
				return Character != null;
			}
			return false;
		}
	}

	public bool HasBackpackSpawned
	{
		get
		{
			if (Owner.UISettings.ShowBackpack)
			{
				return Character.EquipmentEntities.SelectMany((EquipmentEntity ee) => ee.OutfitParts).Any((EquipmentEntity.OutfitPart p) => p.Special == EquipmentEntity.OutfitPartSpecialType.Backpack && (p.StaysInPeacefulMode || !Character.PeacefulMode));
			}
			return false;
		}
	}

	private bool ShouldSquashCloak
	{
		get
		{
			if (!HasQuiverSpawned)
			{
				return HasBackpackSpawned;
			}
			return true;
		}
	}

	public event Action OnModelSpawned;

	public WeaponSet GetSelectedWeaponSet()
	{
		if (!Sets.TryGetValue(Owner.Body.CurrentHandsEquipmentSet, out var value))
		{
			throw new Exception("Can't find visual data for HandsEquipmentSet");
		}
		return value;
	}

	public UnitViewHandsEquipment(UnitEntityView owner, Character character)
	{
		View = owner;
		Character = character;
		if (Owner.CombatState.IsInCombat)
		{
			m_ShouldBeInCombat.Value = Owner.CombatState.IsInCombat;
		}
		if (Owner.Body.InCombatVisual && !Owner.IsInCompanionRoster())
		{
			m_ShouldBeInCombatVisual.Value = Owner.Body.InCombatVisual;
		}
		m_IsActive = (bool)character && !character.IsCreatureAsCharacter;
		if (Active)
		{
			for (int i = 0; i < Owner.Body.HandsEquipmentSets.Count; i++)
			{
				HandsEquipmentSet key = Owner.Body.HandsEquipmentSets[i];
				UnitViewHandSlotData mainHand = new UnitViewHandSlotData(this, i, isMainHand: true);
				UnitViewHandSlotData offHand = new UnitViewHandSlotData(this, i, isMainHand: false);
				Sets.Add(key, new WeaponSet(mainHand, offHand));
			}
			m_ActiveSet = GetSelectedWeaponSet();
			character.OutfitFilter = ShouldShowOutfit;
			character.OnBackEquipmentUpdated += ReattachBackEquipment;
			Owner.UISettings.SubscribeOnBackpackVisibilityChange(UpdateBackpackVisibility);
			Owner.UISettings.SubscribeOnHelmetVisibilityChange(UpdateHelmetVisibility);
			Owner.UISettings.SubscribeOnHelmetVisibilityAboveAllChange(UpdateHelmetVisibilityAboveAll);
			UpdateHelmetVisibility();
			UpdateHelmetVisibilityAboveAll();
			UpdateBackpackVisibility();
			MatchWithCurrentCombatState();
			ForceEndChangeEquipment();
		}
	}

	public void Dispose()
	{
		if (Owner != null && Active)
		{
			Owner.UISettings.UnsubscribeFromBackpackVisibilityChange(UpdateBackpackVisibility);
			Owner.UISettings.UnsubscribeFromHelmetVisibilityChange(UpdateHelmetVisibility);
			Owner.UISettings.UnsubscribeFromHelmetVisibilityAboveAllChange(UpdateHelmetVisibilityAboveAll);
		}
	}

	private void UpdateBackpackVisibility()
	{
		Character.UpdateBackpackVisibility(Owner.UISettings.ShowBackpack);
	}

	private void UpdateHelmetVisibility()
	{
		Character.UpdateHelmetVisibility(Owner.UISettings.ShowHelm);
	}

	private void UpdateHelmetVisibilityAboveAll()
	{
		Character.UpdateHelmetVisibilityAboveAll(Owner.UISettings.ShowHelmAboveAll);
	}

	public void RaiseModelSpawned()
	{
		this.OnModelSpawned?.Invoke();
	}

	[CanBeNull]
	private UnitViewHandSlotData GetSlotData(HandSlot s)
	{
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set in Sets)
		{
			if (set.Key.PrimaryHand == s)
			{
				return set.Value.MainHand;
			}
			if (set.Key.SecondaryHand == s)
			{
				return set.Value.OffHand;
			}
		}
		return null;
	}

	public void HandleEquipmentSlotUpdated([NotNull] HandSlot slot, ItemEntity previousItem)
	{
		using (ProfileScope.NewScope("HandleEquipmentSlotUpdated"))
		{
			if (Active && GetSlotData(slot) != null)
			{
				if (InCombat && Owner.State.CanAct && slot.Active && !IsDollRoom)
				{
					StartCombatChangeAnimation();
				}
				else
				{
					ChangeEquipmentWithoutAnimation();
				}
			}
		}
	}

	public void HandleEquipmentSetChanged()
	{
		if (Active)
		{
			if (InCombat && Owner.State.CanAct && !IsDollRoom)
			{
				StartCombatChangeAnimation();
			}
			else
			{
				UpdateActiveWeaponSetImmediately();
			}
		}
	}

	private void UpdateActiveWeaponSetImmediately()
	{
		WeaponSet activeSet = m_ActiveSet;
		m_ActiveSet = GetSelectedWeaponSet();
		if (IsDollRoom)
		{
			m_ActiveSet.MainHand.AttachModel(InCombat);
			m_ActiveSet.OffHand.AttachModel(InCombat);
		}
		m_ActiveSet.MainHand.MatchVisuals();
		m_ActiveSet.OffHand.MatchVisuals();
		RedistributeSlots();
		if (IsDollRoom)
		{
			activeSet.MainHand.AttachModel(!InCombat);
			activeSet.OffHand.AttachModel(!InCombat);
		}
		activeSet.MainHand.MatchVisuals();
		activeSet.OffHand.MatchVisuals();
	}

	public void StartCombatChangeAnimation()
	{
		if (!m_IsAnimatingWeaponsChange)
		{
			if (m_Coroutine.IsRunning)
			{
				InterruptAnimation();
			}
			AbstractUnitCommand current = Owner.Commands.Current;
			if (current != null && !IsDollRoom)
			{
				current.Params.InterruptAsSoonAsPossible = true;
			}
			m_Coroutine = Game.Instance.CoroutinesController.Start(AnimateEquipmentChangeInCombat());
		}
	}

	public void ForceEndChangeEquipment()
	{
		InterruptAnimation();
		if (m_ActiveSet != null && Owner != null)
		{
			ChangeEquipmentWithoutAnimation();
		}
	}

	private void InterruptAnimation()
	{
		if (m_Coroutine.IsRunning)
		{
			Game.Instance.CoroutinesController.Stop(m_Coroutine);
		}
		foreach (HandEquipmentHelper handAnimation in HandAnimations)
		{
			handAnimation.Dispose();
		}
		HandAnimations.Clear();
		if ((bool)AreHandsBusyWithAnimation)
		{
			AreHandsBusyWithAnimation.Value = false;
		}
		m_IsAnimatingWeaponsChange = false;
	}

	private IEnumerator AnimateEquipmentChangeInCombat()
	{
		AreHandsBusyWithAnimation.Value = true;
		m_IsAnimatingWeaponsChange = true;
		if (!IsDollRoom)
		{
			do
			{
				yield return null;
			}
			while (Owner.CombatState.IsInCombat && !Owner.CombatState.CanActInCombat);
		}
		while (IsMainHandMismatched || IsOffHandMismatched)
		{
			bool isMainHandMismatched = IsMainHandMismatched;
			bool isOffHandMismatched = IsOffHandMismatched;
			WeaponSet nextActiveSet = GetSelectedWeaponSet();
			bool flag = isMainHandMismatched && nextActiveSet.MainHand.Slot.HasItem;
			bool flag2 = isOffHandMismatched && nextActiveSet.OffHand.Slot.HasItem;
			HandEquipmentHelper handEquipmentHelper = (isOffHandMismatched ? m_ActiveSet.OffHand.Unequip() : null);
			HandEquipmentHelper unequipMain = (isMainHandMismatched ? m_ActiveSet.MainHand.Unequip() : null);
			UnitEquipmentVisualSlotType equipMainFrom = UnitEquipmentVisualSlotType.None;
			UnitEquipmentVisualSlotType equipOffFrom = UnitEquipmentVisualSlotType.None;
			if (m_ActiveSet == nextActiveSet && flag)
			{
				BlueprintItemEquipmentHand blueprintItemEquipmentHand = (BlueprintItemEquipmentHand)nextActiveSet.MainHand.Slot.Item.Blueprint;
				equipMainFrom = blueprintItemEquipmentHand.VisualParameters.AttachSlots.FirstOrDefault();
			}
			if (m_ActiveSet == nextActiveSet && flag2)
			{
				BlueprintItemEquipmentHand blueprintItemEquipmentHand2 = (BlueprintItemEquipmentHand)nextActiveSet.OffHand.Slot.Item.Blueprint;
				equipOffFrom = blueprintItemEquipmentHand2.VisualParameters.AttachSlots.FirstOrDefault((UnitEquipmentVisualSlotType s) => s.IsRight());
			}
			HandEquipmentHelper equipMain = (flag ? nextActiveSet.MainHand.Equip(equipMainFrom) : null);
			HandEquipmentHelper equipOff = (flag2 ? nextActiveSet.OffHand.Equip(equipOffFrom) : null);
			if (handEquipmentHelper != null)
			{
				yield return handEquipmentHelper;
			}
			if (unequipMain != null)
			{
				yield return unequipMain;
			}
			if (m_ActiveSet.MainHand.IsMismatched)
			{
				if (equipMainFrom != 0)
				{
					m_ActiveSet.MainHand.VisualSlot = equipMainFrom;
				}
				m_ActiveSet.MainHand.MatchVisuals();
			}
			if (m_ActiveSet.OffHand.IsMismatched)
			{
				if (equipOffFrom != 0)
				{
					m_ActiveSet.OffHand.VisualSlot = equipOffFrom;
				}
				m_ActiveSet.OffHand.MatchVisuals();
			}
			m_ActiveSet = nextActiveSet;
			RedistributeSlots();
			yield return equipMain;
			yield return equipOff;
		}
		AreHandsBusyWithAnimation.Value = false;
		m_IsAnimatingWeaponsChange = false;
		m_Coroutine = default(CoroutineHandler);
		HandAnimations.Clear();
	}

	private void ChangeEquipmentWithoutAnimation()
	{
		if (m_IsAnimatingWeaponsChange)
		{
			return;
		}
		WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
		bool flag = IsMainHandMismatched && selectedWeaponSet.MainHand.Slot.HasItem;
		bool flag2 = IsOffHandMismatched && selectedWeaponSet.OffHand.Slot.HasItem;
		m_ActiveSet.MainHand.AttachModel(toHand: false);
		m_ActiveSet.OffHand.AttachModel(toHand: false);
		UnitEquipmentVisualSlotType unitEquipmentVisualSlotType = UnitEquipmentVisualSlotType.None;
		UnitEquipmentVisualSlotType unitEquipmentVisualSlotType2 = UnitEquipmentVisualSlotType.None;
		if (m_ActiveSet == selectedWeaponSet && flag)
		{
			unitEquipmentVisualSlotType = ((BlueprintItemEquipmentHand)selectedWeaponSet.MainHand.Slot.Item.Blueprint).VisualParameters.AttachSlots.FirstOrDefault();
		}
		if (m_ActiveSet == selectedWeaponSet && flag2)
		{
			unitEquipmentVisualSlotType2 = ((BlueprintItemEquipmentHand)selectedWeaponSet.OffHand.Slot.Item.Blueprint).VisualParameters.AttachSlots.FirstOrDefault((UnitEquipmentVisualSlotType s) => s.IsRight());
		}
		selectedWeaponSet.MainHand.AttachModel(InCombat);
		selectedWeaponSet.OffHand.AttachModel(InCombat);
		if (m_ActiveSet.MainHand.IsMismatched)
		{
			if (unitEquipmentVisualSlotType != 0)
			{
				m_ActiveSet.MainHand.VisualSlot = unitEquipmentVisualSlotType;
			}
			m_ActiveSet.MainHand.MatchVisuals();
		}
		if (m_ActiveSet.OffHand.IsMismatched)
		{
			if (unitEquipmentVisualSlotType2 != 0)
			{
				m_ActiveSet.OffHand.VisualSlot = unitEquipmentVisualSlotType2;
			}
			m_ActiveSet.OffHand.MatchVisuals();
		}
		m_ActiveSet = selectedWeaponSet;
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set in Sets)
		{
			if (set.Value != m_ActiveSet)
			{
				if (set.Value.MainHand.IsMismatched)
				{
					set.Value.MainHand.MatchVisuals();
				}
				if (set.Value.OffHand.IsMismatched)
				{
					set.Value.OffHand.MatchVisuals();
				}
			}
		}
		RedistributeSlots();
	}

	private void RedistributeSlots()
	{
		for (int i = 0; i < m_SlotsByVisualSlot.Length; i++)
		{
			UnitViewHandSlotData unitViewHandSlotData = m_SlotsByVisualSlot[i];
			if (unitViewHandSlotData != null && unitViewHandSlotData.VisualSlot != (UnitEquipmentVisualSlotType)i)
			{
				unitViewHandSlotData.VisualSlot = UnitEquipmentVisualSlotType.None;
				m_SlotsByVisualSlot[i] = null;
			}
		}
		List<UnitEquipmentVisualSlotType> possibleSlots = new List<UnitEquipmentVisualSlotType>();
		FindSlotForHand(m_ActiveSet.MainHand, possibleSlots, force: true);
		FindSlotForHand(m_ActiveSet.OffHand, possibleSlots, force: true);
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set in Sets)
		{
			if (set.Value != m_ActiveSet)
			{
				FindSlotForHand(set.Value.MainHand, possibleSlots);
				FindSlotForHand(set.Value.OffHand, possibleSlots);
			}
		}
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set2 in Sets)
		{
			if (!set2.Value.MainHand.VisualModel)
			{
				set2.Value.MainHand.MatchVisuals();
			}
			else
			{
				set2.Value.MainHand.AttachModel();
			}
			if (!set2.Value.OffHand.VisualModel)
			{
				set2.Value.OffHand.MatchVisuals();
			}
			else
			{
				set2.Value.OffHand.AttachModel();
			}
		}
		UpdateBeltPrefabs();
	}

	private void ReattachBackEquipment()
	{
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set in Sets)
		{
			if (set.Value.MainHand.VisualSlot.IsBack())
			{
				set.Value.MainHand.MatchVisuals();
			}
			if (set.Value.OffHand.VisualSlot.IsBack())
			{
				set.Value.OffHand.MatchVisuals();
			}
		}
	}

	public void UpdateBeltPrefabs()
	{
		if (!m_IsActive)
		{
			return;
		}
		using (ProfileScope.NewScope("UpdateBeltPrefabs"))
		{
			int i = 0;
			bool flag = false;
			if (!Character.PeacefulMode && !Character.ForbidBeltItemVisualization)
			{
				foreach (UsableSlot item in from s in Owner.Body.QuickSlots
					where s.HasItem && (bool)s.Item.Blueprint.BeltItemPrefab
					orderby (s.Item.Blueprint.Type != UsableItemType.Utility) ? 1 : 0
					select s)
				{
					for (; i < s_ConsumableVisualSlots.Length && m_SlotsByVisualSlot[(int)s_ConsumableVisualSlots[i]] != null; i++)
					{
						if ((bool)m_ConsumableSlots[i])
						{
							UnityEngine.Object.Destroy(m_ConsumableSlots[i]);
							m_ConsumableSlots[i] = null;
						}
					}
					if (i >= s_ConsumableVisualSlots.Length)
					{
						break;
					}
					if ((bool)m_ConsumableSlots[i])
					{
						UnityEngine.Object.Destroy(m_ConsumableSlots[i]);
					}
					UnitEquipmentVisualSlotType unitEquipmentVisualSlotType = s_ConsumableVisualSlots[i];
					Transform visualSlotBone = GetVisualSlotBone(unitEquipmentVisualSlotType);
					GameObject gameObject = UnityEngine.Object.Instantiate(item.Item.Blueprint.BeltItemPrefab, visualSlotBone, worldPositionStays: false);
					EquipmentOffsets component = gameObject.GetComponent<EquipmentOffsets>();
					component = (component ? component : Game.Instance.BlueprintRoot.Prefabs.DefaultConsumableOffsets);
					if ((bool)component)
					{
						component.Apply(unitEquipmentVisualSlotType, isOffHand: false, Character, gameObject.transform);
					}
					gameObject.transform.localScale *= Owner.View.GetSizeScale();
					m_ConsumableSlots[i] = gameObject;
					i++;
					flag = true;
				}
			}
			for (int j = i; j < m_ConsumableSlots.Length; j++)
			{
				if ((bool)m_ConsumableSlots[j])
				{
					UnityEngine.Object.Destroy(m_ConsumableSlots[j]);
					m_ConsumableSlots[j] = null;
					flag = true;
				}
			}
			Character.FilterOutfit();
			UpdateLightList();
			if (flag)
			{
				Owner.View.MarkRenderersAndCollidersAreUpdated();
				RaiseModelSpawned();
			}
		}
	}

	private bool ShouldShowOutfit(EquipmentEntity.OutfitPart part, GameObject gameObject)
	{
		if (part.Special == EquipmentEntity.OutfitPartSpecialType.Backpack)
		{
			return Owner.UISettings.ShowBackpack;
		}
		if (part.Special == EquipmentEntity.OutfitPartSpecialType.Cloak || part.Special == EquipmentEntity.OutfitPartSpecialType.CloakSquashed)
		{
			if ((part.Special == EquipmentEntity.OutfitPartSpecialType.Cloak) ^ !ShouldSquashCloak)
			{
				return false;
			}
			if (m_PrevCloakObj != gameObject && (bool)m_PrevCloakObj)
			{
				m_PrevCloakObj.SetActive(value: false);
			}
			m_PrevCloakObj = gameObject;
			return true;
		}
		Transform parent = gameObject.transform.parent;
		for (int i = 0; i < s_ConsumableVisualSlots.Length; i++)
		{
			if (i < s_ConsumableVisualSlots.Length)
			{
				if (s_ConsumableVisualSlots[i].GetBoneName() != parent.name && s_ConsumableVisualSlots[i].GetBoneName() != parent.parent?.name)
				{
					continue;
				}
				UnitViewHandSlotData unitViewHandSlotData = m_SlotsByVisualSlot[(int)s_ConsumableVisualSlots[i]];
				if (unitViewHandSlotData != null)
				{
					if (!unitViewHandSlotData.IsInHand || !unitViewHandSlotData.VisualModel)
					{
						return !unitViewHandSlotData.SheathVisualModel;
					}
					return false;
				}
			}
			if ((bool)m_ConsumableSlots[i])
			{
				return false;
			}
		}
		return true;
	}

	private void FindSlotForHand(UnitViewHandSlotData handSlot, List<UnitEquipmentVisualSlotType> possibleSlots, bool force = false)
	{
		if (handSlot.VisibleItem == null || handSlot.VisualSlot != 0)
		{
			return;
		}
		possibleSlots.Clear();
		handSlot.GetPossibleVisualSlots(possibleSlots);
		foreach (UnitEquipmentVisualSlotType possibleSlot in possibleSlots)
		{
			if (m_SlotsByVisualSlot[(int)possibleSlot] == null)
			{
				m_SlotsByVisualSlot[(int)possibleSlot] = handSlot;
				handSlot.VisualSlot = possibleSlot;
				break;
			}
		}
		if (handSlot.VisualSlot == UnitEquipmentVisualSlotType.None && force && possibleSlots.Count > 0)
		{
			if (!IsDollRoom || !handSlot.IsActiveSet)
			{
				m_SlotsByVisualSlot[(int)possibleSlots[0]].VisualSlot = UnitEquipmentVisualSlotType.None;
				m_SlotsByVisualSlot[(int)possibleSlots[0]] = handSlot;
			}
			handSlot.VisualSlot = possibleSlots[0];
		}
	}

	public void UpdateAll()
	{
		if (!Active)
		{
			return;
		}
		foreach (HandsEquipmentSet handsEquipmentSet in Owner.Body.HandsEquipmentSets)
		{
			GetSlotData(handsEquipmentSet.PrimaryHand)?.MatchVisuals();
			GetSlotData(handsEquipmentSet.SecondaryHand)?.MatchVisuals();
		}
		RedistributeSlots();
		UpdateLocatorTrackers();
	}

	public void SetCombatState(bool inCombat)
	{
		if (Active)
		{
			m_ShouldBeInCombat.Value = inCombat;
			if (Owner.IsInPlayerParty && !inCombat)
			{
				m_ShouldBeInCombatVisual.Value = false;
			}
			Refresh();
		}
	}

	public void SetCombatVisualState(bool inCombat)
	{
		if (Active)
		{
			m_ShouldBeInCombatVisual.Value = inCombat;
			Owner.Body.InCombatVisual = m_ShouldBeInCombatVisual.Value;
			Refresh();
		}
	}

	private void Refresh()
	{
		if (Game.Instance.HandsEquipmentController != null)
		{
			Game.Instance.HandsEquipmentController.ScheduleUpdate(this);
		}
		else
		{
			MatchWithCurrentCombatState();
		}
	}

	public bool MatchWithCurrentCombatState()
	{
		if (InCombat == ShouldBeInCombat)
		{
			return true;
		}
		if (!Owner.State.CanAct && !IsDollRoom)
		{
			if (Game.Instance.HandsEquipmentController != null)
			{
				Game.Instance.HandsEquipmentController.ScheduleUpdate(this);
			}
			return false;
		}
		InCombat = ShouldBeInCombat;
		if ((InCombat && m_ActiveSet != GetSelectedWeaponSet()) || IsDollRoom)
		{
			UpdateActiveWeaponSetImmediately();
		}
		if (m_IsAnimatingWeaponsChange)
		{
			m_ActiveSet.OffHand.MatchVisuals();
			m_ActiveSet.OffHand.AttachModel(!InCombat);
			m_ActiveSet.MainHand.MatchVisuals();
			m_ActiveSet.MainHand.AttachModel(!InCombat);
		}
		if (m_Coroutine.IsRunning)
		{
			InterruptAnimation();
		}
		if (!IsDollRoom)
		{
			m_Coroutine = Game.Instance.CoroutinesController.Start(AnimateEquipping(InCombat));
		}
		else
		{
			m_ActiveSet.OffHand.MatchVisuals();
			m_ActiveSet.OffHand.AttachModel(InCombat);
			m_ActiveSet.MainHand.MatchVisuals();
			m_ActiveSet.MainHand.AttachModel(InCombat);
		}
		return true;
	}

	private IEnumerator AnimateEquipping(bool equip)
	{
		AreHandsBusyWithAnimation.Value = true;
		UnitViewHandSlotData mainHand = m_ActiveSet.MainHand;
		UnitViewHandSlotData offHand = m_ActiveSet.OffHand;
		if (equip)
		{
			HandEquipmentHelper handEquipmentHelper = (mainHand.CanAnimate ? mainHand.Equip() : null);
			HandEquipmentHelper oh = (offHand.CanAnimate ? offHand.Equip() : null);
			float num = (Owner.CombatState.IsInCombat ? 0.4f : 100f);
			float num2 = (handEquipmentHelper?.Duration + oh?.Duration) ?? num;
			float timeScale = Mathf.Max(num2 / num, 1f);
			if (Owner.State.IsCharging)
			{
				timeScale = Mathf.Max(timeScale, 1.5f);
			}
			if (handEquipmentHelper != null)
			{
				handEquipmentHelper.Scale(timeScale);
				yield return handEquipmentHelper;
			}
			if (oh != null)
			{
				oh.Scale(timeScale);
				yield return oh;
			}
		}
		else
		{
			HandEquipmentHelper handEquipmentHelper2 = (offHand.CanAnimate ? offHand.Unequip() : null);
			HandEquipmentHelper oh = (mainHand.CanAnimate ? mainHand.Unequip() : null);
			yield return handEquipmentHelper2;
			yield return oh;
		}
		if (IsDollRoom)
		{
			Character.RebuildOutfit();
		}
		AreHandsBusyWithAnimation.Value = false;
		m_Coroutine = default(CoroutineHandler);
		HandAnimations.Clear();
		if (!equip)
		{
			UpdateActiveWeaponSetImmediately();
		}
	}

	public void ForceSwitch(bool inCombat)
	{
		if (Active)
		{
			InCombat = inCombat;
			Owner.Body.InCombatVisual = InCombat;
			m_ActiveSet.MainHand.IsInHand = inCombat && m_ActiveSet.MainHand.Slot.Active;
			m_ActiveSet.MainHand.MatchVisuals();
			m_ActiveSet.MainHand.AttachModel();
			m_ActiveSet.OffHand.IsInHand = inCombat && m_ActiveSet.OffHand.Slot.Active;
			m_ActiveSet.OffHand.MatchVisuals();
			m_ActiveSet.OffHand.AttachModel();
		}
	}

	public void OnLeaveProne()
	{
		if (Active && InCombat != ShouldBeInCombat)
		{
			if (ShouldBeInCombat)
			{
				ForceSwitch(ShouldBeInCombat);
			}
			else
			{
				Game.Instance.HandsEquipmentController.ScheduleUpdate(this);
			}
		}
	}

	public GameObject GetWeaponModel(bool offHand)
	{
		if (!Active)
		{
			return null;
		}
		return (offHand ? m_ActiveSet.OffHand : m_ActiveSet.MainHand).VisualModel;
	}

	private void UpdateLocatorTrackers()
	{
		if (m_OffHandTracker == null)
		{
			m_OffHandTracker = GetWeaponModel(offHand: true)?.AddComponent<LocatorPositionTracker>();
		}
		if (m_MainHandTracker == null)
		{
			m_MainHandTracker = GetWeaponModel(offHand: false)?.AddComponent<LocatorPositionTracker>();
		}
	}

	public Quaternion GetWeaponRotation()
	{
		_ = Active;
		return Owner.View.ViewTransform.rotation;
	}

	public void UpdateVisibility(bool isVisible)
	{
		if (!Active)
		{
			return;
		}
		foreach (KeyValuePair<HandsEquipmentSet, WeaponSet> set in Sets)
		{
			set.Value.MainHand.ShowItem(isVisible && !Character.PeacefulMode);
			set.Value.OffHand.ShowItem(isVisible && !Character.PeacefulMode);
		}
		foreach (Light equipmentLight in m_EquipmentLights)
		{
			if ((bool)equipmentLight)
			{
				equipmentLight.gameObject.SetActive(isVisible);
			}
		}
	}

	public void HideOffWeapon(bool hide)
	{
		if (!Active)
		{
			return;
		}
		if (hide)
		{
			if ((bool)m_ActiveSet.OffHand.VisualModel && m_ActiveSet.OffHand.IsInHand)
			{
				m_ActiveSet.OffHand.DestroyModel();
			}
		}
		else
		{
			m_ActiveSet.OffHand.MatchVisuals();
			m_ActiveSet.OffHand.AttachModel();
		}
	}

	public Transform GetVisualSlotBone(UnitEquipmentVisualSlotType visualSlot)
	{
		if (visualSlot == UnitEquipmentVisualSlotType.None)
		{
			return null;
		}
		if (!m_BonesByVisualSlot[(int)visualSlot])
		{
			m_BonesByVisualSlot[(int)visualSlot] = Character.transform.FindChildRecursive(visualSlot.GetBoneName());
		}
		return m_BonesByVisualSlot[(int)visualSlot];
	}

	private void UpdateLightList()
	{
		m_EquipmentLights.Clear();
		GameObject[] consumableSlots = m_ConsumableSlots;
		foreach (GameObject gameObject in consumableSlots)
		{
			if ((bool)gameObject)
			{
				Light componentInChildren = gameObject.GetComponentInChildren<Light>();
				if ((bool)componentInChildren)
				{
					m_EquipmentLights.Add(componentInChildren);
				}
			}
		}
	}

	public IEnumerable<Light> GetAllLights()
	{
		return m_EquipmentLights;
	}
}
