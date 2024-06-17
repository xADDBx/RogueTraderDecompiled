using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items.Slots;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.FX;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility.Locator;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

public class PartUnitBody : BaseUnitPart, IUnitInventoryChanged<EntitySubscriber>, IUnitInventoryChanged, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitInventoryChanged, EntitySubscriber>, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitBody>, IEntityPartOwner
	{
		PartUnitBody Body { get; }
	}

	[JsonProperty]
	private List<ItemEntityWeapon> m_EmptyHandWeaponsStack;

	[JsonProperty]
	private ItemEntityWeapon m_EmptyHandWeapon;

	[JsonProperty]
	private int m_CurrentHandsEquipmentSetIndex;

	[JsonProperty]
	private HandsEquipmentSet[] m_HandsEquipmentSets;

	[JsonProperty]
	private readonly List<WeaponSlot> m_AdditionalLimbs = new List<WeaponSlot>();

	[JsonProperty]
	[CanBeNull]
	private HandsEquipmentSet m_PolymorphHandsEquipmentSet;

	[JsonProperty]
	[CanBeNull]
	private List<WeaponSlot> m_PolymorphAdditionalLimbs;

	[JsonProperty]
	private bool m_PolymorphKeepSlots;

	[JsonProperty(PropertyName = "m_QuickSlots")]
	public UsableSlot[] QuickSlots;

	[JsonProperty]
	public ArmorSlot Armor;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentShirt> Shirt;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentBelt> Belt;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentHead> Head;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentGlasses> Glasses;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentFeet> Feet;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentGloves> Gloves;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentNeck> Neck;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentRing> Ring1;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentRing> Ring2;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentWrist> Wrist;

	[JsonProperty]
	public EquipmentSlot<BlueprintItemEquipmentShoulders> Shoulders;

	[JsonProperty(PropertyName = "m_Mechadendrites")]
	public List<EquipmentSlot<BlueprintItemMechadendrite>> Mechadendrites = new List<EquipmentSlot<BlueprintItemMechadendrite>>();

	private bool m_EquipmentWeightDirty = true;

	private float m_EquipmentWeight;

	[CanBeNull]
	private HandsEquipmentSet m_CutsceneHandsEquipmentSet;

	public readonly List<ItemSlot> EquipmentSlots = new List<ItemSlot>();

	public readonly List<ItemSlot> AllSlots = new List<ItemSlot>();

	[JsonProperty]
	public bool HandsAreEnabled { get; private set; }

	[JsonProperty]
	[GameStateIgnore]
	public bool InCombatVisual { get; set; }

	public bool IsPolymorphed => base.Owner.GetOptional<PartPolymorphed>();

	public bool IsInitializing { get; private set; }

	public float EquipmentWeight
	{
		get
		{
			if (m_EquipmentWeightDirty)
			{
				m_EquipmentWeight = 0f;
				foreach (ItemSlot equipmentSlot in EquipmentSlots)
				{
					m_EquipmentWeight += equipmentSlot.MaybeItem?.TotalWeight ?? 0f;
				}
				m_EquipmentWeightDirty = false;
			}
			return m_EquipmentWeight;
		}
	}

	[CanBeNull]
	public ItemEntityWeapon EmptyHandWeapon => m_EmptyHandWeapon;

	[CanBeNull]
	public List<ItemEntityWeapon> EmptyHandWeaponsStack => m_EmptyHandWeaponsStack;

	public int CurrentHandEquipmentSetIndex
	{
		get
		{
			return m_CurrentHandsEquipmentSetIndex;
		}
		set
		{
			if (value < 0 || value >= 2)
			{
				PFLog.Default.Error("Invalid HandsEquipmentSet index: " + value);
				return;
			}
			int num = Math.Max(0, Math.Min(1, value));
			HandsEquipmentSet currentHandsEquipmentSet = CurrentHandsEquipmentSet;
			if (m_CurrentHandsEquipmentSetIndex != num)
			{
				m_CurrentHandsEquipmentSetIndex = num;
				using (HandSlot.SuppressNotifyEquipment())
				{
					currentHandsEquipmentSet.PrimaryHand.UpdateActive();
					currentHandsEquipmentSet.SecondaryHand.UpdateActive();
					CurrentHandsEquipmentSet.PrimaryHand.UpdateActive();
					CurrentHandsEquipmentSet.SecondaryHand.UpdateActive();
				}
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitActiveEquipmentSetHandler>)delegate(IUnitActiveEquipmentSetHandler h)
				{
					h.HandleUnitChangeActiveEquipmentSet();
				}, isCheckRuntime: true);
				Services.GetInstance<FXPrewarmService>().PrewarmWeaponSet(CurrentHandsEquipmentSet);
			}
		}
	}

	public IList<HandsEquipmentSet> HandsEquipmentSets => m_HandsEquipmentSets;

	public HandsEquipmentSet CurrentHandsEquipmentSet => m_CutsceneHandsEquipmentSet ?? m_PolymorphHandsEquipmentSet ?? m_HandsEquipmentSets[m_CurrentHandsEquipmentSetIndex];

	public bool IsCurrentHandsEquipmentSetPolymorphed => m_PolymorphHandsEquipmentSet != null;

	[CanBeNull]
	public HandsEquipmentSet PolymorphHandsEquipmentSet => m_PolymorphHandsEquipmentSet;

	public IEnumerable<HandSlot> Hands
	{
		get
		{
			yield return PrimaryHand;
			yield return SecondaryHand;
		}
	}

	public List<WeaponSlot> AdditionalLimbs => m_PolymorphAdditionalLimbs ?? m_AdditionalLimbs;

	public HandSlot PrimaryHand => CurrentHandsEquipmentSet.PrimaryHand;

	public HandSlot SecondaryHand => CurrentHandsEquipmentSet.SecondaryHand;

	public IEnumerable<ItemSlot> CurrentEquipmentSlots
	{
		get
		{
			yield return CurrentHandsEquipmentSet.PrimaryHand;
			yield return CurrentHandsEquipmentSet.SecondaryHand;
			yield return Armor;
			yield return Shirt;
			yield return Belt;
			yield return Head;
			yield return Glasses;
			yield return Feet;
			yield return Gloves;
			yield return Neck;
			yield return Ring1;
			yield return Ring2;
			yield return Wrist;
			yield return Shoulders;
		}
	}

	public IEnumerable<ItemEntity> Items => from s in EquipmentSlots
		where s?.HasItem ?? false
		select s.Item;

	private IEnumerable<ItemEntity> AllEmptyHands
	{
		get
		{
			if (m_EmptyHandWeapon != null)
			{
				yield return m_EmptyHandWeapon;
			}
			if (m_EmptyHandWeaponsStack == null)
			{
				yield break;
			}
			foreach (ItemEntityWeapon item in m_EmptyHandWeaponsStack)
			{
				if (item != null)
				{
					yield return item;
				}
			}
		}
	}

	public bool HandsEquipmentAreVisible
	{
		get
		{
			if (IsPolymorphed)
			{
				return m_PolymorphKeepSlots;
			}
			return true;
		}
	}

	protected override void OnAttach()
	{
		HandsAreEnabled = !base.Owner.OriginalBlueprint.Body.DisableHands;
		BlueprintUnit.UnitBody body = base.Owner.OriginalBlueprint.Body;
		if (HandsAreEnabled && (bool)body.EmptyHandWeapon)
		{
			m_EmptyHandWeapon = body.EmptyHandWeapon.CreateEntity<ItemEntityWeapon>();
			m_EmptyHandWeapon.OnDidEquipped(base.Owner);
		}
		m_HandsEquipmentSets = new HandsEquipmentSet[2];
		for (int i = 0; i < m_HandsEquipmentSets.Length; i++)
		{
			m_HandsEquipmentSets[i] = new HandsEquipmentSet(base.Owner);
		}
		int num = body.AdditionalLimbs.Length + body.AdditionalSecondaryLimbs.Length;
		for (int j = 0; j < num; j++)
		{
			m_AdditionalLimbs.Add(new WeaponSlot(base.Owner));
		}
		QuickSlots = new UsableSlot[body.QuickSlots.Length];
		for (int k = 0; k < QuickSlots.Length; k++)
		{
			QuickSlots[k] = new UsableSlot(base.Owner);
		}
		for (int l = 0; l < body.Mechadendrites.Length; l++)
		{
			Mechadendrites.Add(new EquipmentSlot<BlueprintItemMechadendrite>(base.Owner));
		}
		Armor = new ArmorSlot(base.Owner);
		Shirt = new EquipmentSlot<BlueprintItemEquipmentShirt>(base.Owner);
		Belt = new EquipmentSlot<BlueprintItemEquipmentBelt>(base.Owner);
		Head = new EquipmentSlot<BlueprintItemEquipmentHead>(base.Owner);
		Glasses = new EquipmentSlot<BlueprintItemEquipmentGlasses>(base.Owner);
		Feet = new EquipmentSlot<BlueprintItemEquipmentFeet>(base.Owner);
		Gloves = new EquipmentSlot<BlueprintItemEquipmentGloves>(base.Owner);
		Neck = new EquipmentSlot<BlueprintItemEquipmentNeck>(base.Owner);
		Ring1 = new EquipmentSlot<BlueprintItemEquipmentRing>(base.Owner);
		Ring2 = new EquipmentSlot<BlueprintItemEquipmentRing>(base.Owner);
		Wrist = new EquipmentSlot<BlueprintItemEquipmentWrist>(base.Owner);
		Shoulders = new EquipmentSlot<BlueprintItemEquipmentShoulders>(base.Owner);
		CollectAllSlots();
	}

	protected override void OnViewDidAttach()
	{
		Services.GetInstance<FXPrewarmService>().PrewarmWeaponSet(CurrentHandsEquipmentSet);
	}

	private void CollectAllSlots()
	{
		MarkEquipmentWeightDirty();
		EquipmentSlots.Clear();
		AllSlots.Clear();
		HandsEquipmentSet[] handsEquipmentSets = m_HandsEquipmentSets;
		foreach (HandsEquipmentSet handsEquipmentSet in handsEquipmentSets)
		{
			AddEquipmentSlot(handsEquipmentSet.PrimaryHand);
			AddEquipmentSlot(handsEquipmentSet.SecondaryHand);
		}
		AddEquipmentSlot(Armor);
		AddEquipmentSlot(Shirt);
		AddEquipmentSlot(Belt);
		AddEquipmentSlot(Head);
		AddEquipmentSlot(Glasses);
		AddEquipmentSlot(Feet);
		AddEquipmentSlot(Gloves);
		AddEquipmentSlot(Neck);
		AddEquipmentSlot(Ring1);
		AddEquipmentSlot(Ring2);
		AddEquipmentSlot(Wrist);
		AddEquipmentSlot(Shoulders);
		UsableSlot[] quickSlots = QuickSlots;
		foreach (UsableSlot slot2 in quickSlots)
		{
			AddEquipmentSlot(slot2);
		}
		foreach (EquipmentSlot<BlueprintItemMechadendrite> mechadendrite in Mechadendrites)
		{
			AddEquipmentSlot(mechadendrite);
		}
		foreach (WeaponSlot additionalLimb in m_AdditionalLimbs)
		{
			AllSlots.Add(additionalLimb);
		}
		if (m_PolymorphAdditionalLimbs != null)
		{
			foreach (WeaponSlot polymorphAdditionalLimb in m_PolymorphAdditionalLimbs)
			{
				AllSlots.Add(polymorphAdditionalLimb);
			}
		}
		if (m_PolymorphHandsEquipmentSet != null)
		{
			AllSlots.Add(m_PolymorphHandsEquipmentSet.PrimaryHand);
			AllSlots.Add(m_PolymorphHandsEquipmentSet.SecondaryHand);
		}
		void AddEquipmentSlot(ItemSlot slot)
		{
			EquipmentSlots.Add(slot);
			AllSlots.Add(slot);
		}
	}

	public void Initialize()
	{
		try
		{
			IsInitializing = true;
			BlueprintUnit.UnitBody body = base.Owner.OriginalBlueprint.Body;
			for (int i = 0; i < body.AdditionalLimbs.Length; i++)
			{
				if (!HandsAreEnabled && m_HandsEquipmentSets[0].PrimaryHand.MaybeItem == null)
				{
					TryInsertItem(body.AdditionalLimbs[i], m_HandsEquipmentSets[0].PrimaryHand);
				}
				else
				{
					TryInsertItem(body.AdditionalLimbs[i], m_AdditionalLimbs[i]);
				}
			}
			int num = 0;
			int num2 = body.AdditionalLimbs.Length;
			while (num < body.AdditionalSecondaryLimbs.Length)
			{
				TryInsertItem(body.AdditionalSecondaryLimbs[num], m_AdditionalLimbs[num2]);
				if (m_AdditionalLimbs[num2]?.MaybeItem is ItemEntityWeapon itemEntityWeapon)
				{
					itemEntityWeapon.ForceSecondary = true;
				}
				num++;
				num2++;
			}
			for (int j = 0; j < body.QuickSlots.Length; j++)
			{
				TryInsertItem(body.QuickSlots[j], QuickSlots[j]);
			}
			for (int k = 0; k < body.Mechadendrites.Length; k++)
			{
				TryInsertItem(body.Mechadendrites[k], Mechadendrites[k]);
			}
			TryInsertItem(body.Armor, Armor);
			TryInsertItem(body.Shirt, Shirt);
			TryInsertItem(body.Belt, Belt);
			TryInsertItem(body.Head, Head);
			TryInsertItem(body.Glasses, Glasses);
			TryInsertItem(body.Feet, Feet);
			TryInsertItem(body.Gloves, Gloves);
			TryInsertItem(body.Neck, Neck);
			TryInsertItem(body.Ring1, Ring1);
			TryInsertItem(body.Ring2, Ring2);
			TryInsertItem(body.Wrist, Wrist);
			TryInsertItem(body.Shoulders, Shoulders);
		}
		finally
		{
			IsInitializing = false;
		}
	}

	public void InitializeWeapons(BlueprintUnit.UnitBody eq)
	{
		try
		{
			IsInitializing = true;
			if (HandsAreEnabled)
			{
				m_CurrentHandsEquipmentSetIndex = ((eq.ActiveHandSet >= 0 && eq.ActiveHandSet < 2) ? eq.ActiveHandSet : 0);
				for (int i = 0; i < 2; i++)
				{
					BlueprintItemEquipmentHand handEquipment = eq.GetHandEquipment(i, main: true);
					BlueprintItemEquipmentHand handEquipment2 = eq.GetHandEquipment(i, main: false);
					m_HandsEquipmentSets[i].PrimaryHand.UpdateActive();
					m_HandsEquipmentSets[i].SecondaryHand.UpdateActive();
					TryInsertItem(handEquipment, m_HandsEquipmentSets[i].PrimaryHand);
					TryInsertItem(handEquipment2, m_HandsEquipmentSets[i].SecondaryHand);
				}
			}
		}
		finally
		{
			IsInitializing = false;
		}
	}

	public void UpgradeHandsFromBlueprint()
	{
		using (ContextData<ItemSlot.IgnoreLock>.Request())
		{
			HandsAreEnabled = !base.Owner.OriginalBlueprint.Body.DisableHands;
			m_CurrentHandsEquipmentSetIndex = 0;
			HandsEquipmentSet[] handsEquipmentSets = m_HandsEquipmentSets;
			foreach (HandsEquipmentSet handsEquipmentSet in handsEquipmentSets)
			{
				ItemEntity maybeItem = handsEquipmentSet.PrimaryHand.MaybeItem;
				if (maybeItem != null)
				{
					handsEquipmentSet.PrimaryHand.RemoveItem();
					maybeItem.Collection?.Remove(maybeItem);
				}
				maybeItem = handsEquipmentSet.SecondaryHand.MaybeItem;
				if (maybeItem != null)
				{
					handsEquipmentSet.SecondaryHand.RemoveItem();
					maybeItem.Collection?.Remove(maybeItem);
				}
			}
		}
		InitializeWeapons(base.Owner.OriginalBlueprint.Body);
	}

	public void TryInsertItem(BlueprintItem bpItem, ItemSlot slot)
	{
		if (!bpItem || (bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		ItemEntity itemEntity = bpItem.CreateEntity();
		if (!slot.CanInsertItem(itemEntity))
		{
			PFLog.Default.Error("'{0}' can't insert item '{1}' to slot '{2}'", base.Owner.OriginalBlueprint, bpItem, slot.GetType().Name);
			base.Owner.Inventory.Add(itemEntity);
			return;
		}
		if (bpItem is BlueprintItemEquipment && !itemEntity.CanBeEquippedBy(base.Owner))
		{
			PFLog.Default.Error("'{0}' can't equip item '{1}'", base.Owner.OriginalBlueprint, bpItem);
			base.Owner.Inventory.Add(itemEntity);
			return;
		}
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			slot.InsertItem(itemEntity);
		}
	}

	public ItemSlot FindSlotWithItem(ItemEntity item)
	{
		return AllSlots.FirstOrDefault((ItemSlot s) => s.HasItem && s.Item == item);
	}

	public int AddAdditionalLimb(BlueprintItemWeapon weapon, bool isSecondary = false)
	{
		int num = m_AdditionalLimbs.FindIndex((WeaponSlot s) => !s.HasItem);
		if (num < 0)
		{
			m_AdditionalLimbs.Add(new WeaponSlot(base.Owner));
			num = m_AdditionalLimbs.Count - 1;
		}
		ItemEntityWeapon itemEntityWeapon = weapon.CreateEntity<ItemEntityWeapon>();
		itemEntityWeapon.ForceSecondary = isSecondary;
		m_AdditionalLimbs[num].InsertItem(itemEntityWeapon);
		return num;
	}

	public void RemoveAdditionalLimb(int limbIndex)
	{
		if (limbIndex >= 0 && limbIndex < m_AdditionalLimbs.Count && m_AdditionalLimbs[limbIndex].HasItem)
		{
			ItemEntity item = m_AdditionalLimbs[limbIndex].Item;
			m_AdditionalLimbs[limbIndex].RemoveItem();
			base.Owner.Inventory.Remove(item);
		}
	}

	[CanBeNull]
	public WeaponSlot FindWeaponSlot(Predicate<WeaponSlot> pred)
	{
		if (pred(CurrentHandsEquipmentSet.PrimaryHand))
		{
			return CurrentHandsEquipmentSet.PrimaryHand;
		}
		if (pred(CurrentHandsEquipmentSet.SecondaryHand))
		{
			return CurrentHandsEquipmentSet.SecondaryHand;
		}
		foreach (WeaponSlot additionalLimb in AdditionalLimbs)
		{
			if (pred(additionalLimb))
			{
				return additionalLimb;
			}
		}
		return null;
	}

	[CanBeNull]
	public HandsEquipmentSet GetHandsEquipmentSet(HandSlot slot)
	{
		if (m_PolymorphHandsEquipmentSet != null && (m_PolymorphHandsEquipmentSet.PrimaryHand == slot || m_PolymorphHandsEquipmentSet.SecondaryHand == slot))
		{
			return m_PolymorphHandsEquipmentSet;
		}
		for (int i = 0; i < m_HandsEquipmentSets.Length; i++)
		{
			HandsEquipmentSet handsEquipmentSet = m_HandsEquipmentSets[i];
			if (handsEquipmentSet.PrimaryHand == slot || handsEquipmentSet.SecondaryHand == slot)
			{
				return handsEquipmentSet;
			}
		}
		if (m_CutsceneHandsEquipmentSet?.PrimaryHand == slot || m_CutsceneHandsEquipmentSet?.SecondaryHand == slot)
		{
			return m_CutsceneHandsEquipmentSet;
		}
		return null;
	}

	public ItemEntityWeapon SetEmptyHandWeapon(BlueprintItemWeapon weapon)
	{
		if (!HandsAreEnabled)
		{
			PFLog.Default.Error("Hands are not enabled, can't set empty hand weapon");
			return null;
		}
		m_EmptyHandWeapon?.OnWillUnequip();
		m_EmptyHandWeaponsStack = m_EmptyHandWeaponsStack ?? new List<ItemEntityWeapon>();
		m_EmptyHandWeaponsStack.Add(m_EmptyHandWeapon);
		m_EmptyHandWeapon = weapon.CreateEntity<ItemEntityWeapon>();
		m_EmptyHandWeapon.OnDidEquipped(base.Owner);
		return m_EmptyHandWeapon;
	}

	public void RemoveEmptyHandWeapon(ItemEntityWeapon weapon)
	{
		if (!HandsAreEnabled)
		{
			PFLog.Default.Error("Hands are not enabled, can't remove empty hand weapon");
			return;
		}
		if (m_EmptyHandWeapon == weapon)
		{
			if (m_EmptyHandWeaponsStack.Empty())
			{
				PFLog.Default.Error("m_EmptyHandWeaponsStack is empty, can't remove empty hand weapon");
				return;
			}
			m_EmptyHandWeapon.OnWillUnequip();
			m_EmptyHandWeapon = m_EmptyHandWeaponsStack.LastItem();
			m_EmptyHandWeapon?.OnDidEquipped(base.Owner);
			m_EmptyHandWeaponsStack.RemoveAt(m_EmptyHandWeaponsStack.Count - 1);
		}
		m_EmptyHandWeaponsStack?.Remove(weapon);
	}

	public void OnItemInserted(ItemEntity item)
	{
		MarkEquipmentWeightDirty();
	}

	public void OnItemRemoved(ItemEntity item)
	{
		MarkEquipmentWeightDirty();
	}

	private IEnumerable<ItemEntity> GetAllItemsInternal()
	{
		foreach (ItemSlot equipmentSlot in EquipmentSlots)
		{
			if (equipmentSlot.MaybeItem != null)
			{
				yield return equipmentSlot.MaybeItem;
			}
		}
		foreach (WeaponSlot additionalLimb in m_AdditionalLimbs)
		{
			if (additionalLimb.MaybeItem != null)
			{
				yield return additionalLimb.MaybeItem;
			}
		}
		if (m_PolymorphAdditionalLimbs != null)
		{
			foreach (WeaponSlot polymorphAdditionalLimb in m_PolymorphAdditionalLimbs)
			{
				if (polymorphAdditionalLimb.MaybeItem != null)
				{
					yield return polymorphAdditionalLimb.MaybeItem;
				}
			}
		}
		if (m_PolymorphHandsEquipmentSet != null)
		{
			if (m_PolymorphHandsEquipmentSet.PrimaryHand.MaybeItem != null)
			{
				yield return m_PolymorphHandsEquipmentSet.PrimaryHand.MaybeItem;
			}
			if (m_PolymorphHandsEquipmentSet.SecondaryHand.MaybeItem != null)
			{
				yield return m_PolymorphHandsEquipmentSet.SecondaryHand.MaybeItem;
			}
		}
		foreach (ItemEntity allEmptyHand in AllEmptyHands)
		{
			yield return allEmptyHand;
		}
	}

	protected override void OnSubscribe()
	{
		foreach (ItemEntity item in GetAllItemsInternal())
		{
			item.Subscribe();
		}
	}

	protected override void OnUnsubscribe()
	{
		foreach (ItemEntity item in GetAllItemsInternal())
		{
			item.Unsubscribe();
		}
	}

	protected override void OnPreSave()
	{
		AllSlots.Where((ItemSlot s) => s.HasItem).ForEach(delegate(ItemSlot s)
		{
			s.Item.PreSave();
		});
		AllEmptyHands.ForEach(delegate(ItemEntity w)
		{
			w?.PreSave();
		});
	}

	protected override void OnPrePostLoad()
	{
		CollectAllSlots();
		AllSlots.ForEach(delegate(ItemSlot s)
		{
			s.PrePostLoad(base.Owner);
		});
		AllEmptyHands.ForEach(delegate(ItemEntity w)
		{
			w?.PrePostLoad();
		});
	}

	protected override void OnPostLoad()
	{
		AllSlots.ForEach(delegate(ItemSlot s)
		{
			s.PostLoad();
		});
		AllEmptyHands.ForEach(delegate(ItemEntity w)
		{
			w?.PostLoad();
		});
	}

	protected override void OnDetach()
	{
		AllSlots.Where((ItemSlot s) => s.HasItem).ForEach(delegate(ItemSlot s)
		{
			s.Item.Dispose();
		});
		AllEmptyHands.ForEach(delegate(ItemEntity w)
		{
			w?.Dispose();
		});
	}

	public void ApplyPolymorphEffect([CanBeNull] BlueprintItemWeapon mainHand, [CanBeNull] BlueprintItemWeapon offHand, BlueprintItemWeapon[] additionalLimbs, BlueprintItemWeapon[] secondaryAdditionalLimbs, bool keepSlots)
	{
		HandsAreEnabled = mainHand != null || offHand != null;
		m_PolymorphKeepSlots = keepSlots;
		if (!keepSlots)
		{
			m_PolymorphHandsEquipmentSet = new HandsEquipmentSet(base.Owner);
		}
		m_PolymorphAdditionalLimbs = new List<WeaponSlot>();
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			foreach (BlueprintItemWeapon blueprintItemWeapon in additionalLimbs)
			{
				if ((bool)blueprintItemWeapon)
				{
					if (!HandsAreEnabled && m_PolymorphHandsEquipmentSet != null && m_PolymorphHandsEquipmentSet.PrimaryHand.MaybeItem == null)
					{
						m_PolymorphHandsEquipmentSet.PrimaryHand.InsertItem(blueprintItemWeapon.CreateEntity());
						continue;
					}
					WeaponSlot weaponSlot = new WeaponSlot(base.Owner);
					weaponSlot.InsertItem(blueprintItemWeapon.CreateEntity());
					m_PolymorphAdditionalLimbs.Add(weaponSlot);
				}
			}
			foreach (BlueprintItemWeapon blueprintItemWeapon2 in secondaryAdditionalLimbs)
			{
				if ((bool)blueprintItemWeapon2)
				{
					ItemEntityWeapon itemEntityWeapon = blueprintItemWeapon2.CreateEntity<ItemEntityWeapon>();
					itemEntityWeapon.ForceSecondary = true;
					WeaponSlot weaponSlot2 = new WeaponSlot(base.Owner);
					weaponSlot2.InsertItem(itemEntityWeapon);
					m_PolymorphAdditionalLimbs.Add(weaponSlot2);
				}
			}
		}
		if (!keepSlots)
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				if ((bool)mainHand)
				{
					m_PolymorphHandsEquipmentSet.PrimaryHand.InsertItem(mainHand.CreateEntity());
				}
				if ((bool)offHand)
				{
					m_PolymorphHandsEquipmentSet.SecondaryHand.InsertItem(offHand.CreateEntity());
				}
			}
			foreach (ItemSlot equipmentSlot in EquipmentSlots)
			{
				equipmentSlot.Lock.Retain();
			}
			UsableSlot[] quickSlots = QuickSlots;
			for (int j = 0; j < quickSlots.Length; j++)
			{
				quickSlots[j].RetainDeactivateFlag();
			}
			Armor.RetainDeactivateFlag();
		}
		CollectAllSlots();
		m_PolymorphHandsEquipmentSet?.PrimaryHand.Lock.Retain();
		m_PolymorphHandsEquipmentSet?.SecondaryHand.Lock.Retain();
	}

	public void CancelPolymorphEffect()
	{
		m_PolymorphHandsEquipmentSet?.PrimaryHand.Lock.Release();
		m_PolymorphHandsEquipmentSet?.SecondaryHand.Lock.Release();
		if (m_PolymorphAdditionalLimbs != null)
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				foreach (WeaponSlot polymorphAdditionalLimb in m_PolymorphAdditionalLimbs)
				{
					polymorphAdditionalLimb.MaybeItem?.Collection.Remove(polymorphAdditionalLimb.MaybeItem);
				}
			}
		}
		if (!m_PolymorphKeepSlots)
		{
			using (ContextData<ItemsCollection.SuppressEvents>.Request())
			{
				m_PolymorphHandsEquipmentSet?.PrimaryHand.MaybeItem?.Collection.Remove(m_PolymorphHandsEquipmentSet.PrimaryHand.MaybeItem);
				m_PolymorphHandsEquipmentSet?.SecondaryHand.MaybeItem?.Collection.Remove(m_PolymorphHandsEquipmentSet.SecondaryHand.MaybeItem);
			}
			CollectAllSlots();
			foreach (ItemSlot equipmentSlot in EquipmentSlots)
			{
				equipmentSlot.Lock.Release();
			}
			UsableSlot[] quickSlots = QuickSlots;
			for (int i = 0; i < quickSlots.Length; i++)
			{
				quickSlots[i].ReleaseDeactivateFlag();
			}
			Armor.ReleaseDeactivateFlag();
		}
		m_PolymorphHandsEquipmentSet = null;
		m_PolymorphAdditionalLimbs = null;
		m_PolymorphKeepSlots = false;
		HandsAreEnabled = !base.Owner.OriginalBlueprint.Body.DisableHands;
	}

	public void MarkEquipmentWeightDirty()
	{
		m_EquipmentWeightDirty = true;
	}

	public void HandleInventoryChanged()
	{
		using (ContextData<ItemsCollection.DoNotRemoveFromSlot>.Request())
		{
			foreach (ItemEntity item in Items)
			{
				if (item.Collection != null && item.Collection != base.Owner.Inventory.Collection)
				{
					item.Collection.Transfer(item, base.Owner.Inventory.Collection);
				}
			}
		}
	}

	public void SetCutsceneHandsEquipment([CanBeNull] ItemEntityWeapon weapon)
	{
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			if (weapon == null)
			{
				m_CutsceneHandsEquipmentSet = null;
				CurrentHandsEquipmentSet.PrimaryHand.RemoveCutsceneItem();
				CurrentHandsEquipmentSet.SecondaryHand.ReleaseDeactivateFlag();
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitActiveEquipmentSetHandler>)delegate(IUnitActiveEquipmentSetHandler h)
				{
					h.HandleUnitChangeActiveEquipmentSet();
				}, isCheckRuntime: true);
			}
			else
			{
				CurrentHandsEquipmentSet.PrimaryHand.InsertCutsceneItem(weapon);
				CurrentHandsEquipmentSet.SecondaryHand.RetainDeactivateFlag();
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitActiveEquipmentSetHandler>)delegate(IUnitActiveEquipmentSetHandler h)
				{
					h.HandleUnitChangeActiveEquipmentSet();
				}, isCheckRuntime: true);
				Services.GetInstance<FXPrewarmService>().PrewarmWeaponSet(CurrentHandsEquipmentSet);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = HandsAreEnabled;
		result.Append(ref val2);
		List<ItemEntityWeapon> emptyHandWeaponsStack = m_EmptyHandWeaponsStack;
		if (emptyHandWeaponsStack != null)
		{
			for (int i = 0; i < emptyHandWeaponsStack.Count; i++)
			{
				Hash128 val3 = ClassHasher<ItemEntityWeapon>.GetHash128(emptyHandWeaponsStack[i]);
				result.Append(ref val3);
			}
		}
		Hash128 val4 = ClassHasher<ItemEntityWeapon>.GetHash128(m_EmptyHandWeapon);
		result.Append(ref val4);
		result.Append(ref m_CurrentHandsEquipmentSetIndex);
		HandsEquipmentSet[] handsEquipmentSets = m_HandsEquipmentSets;
		if (handsEquipmentSets != null)
		{
			for (int j = 0; j < handsEquipmentSets.Length; j++)
			{
				Hash128 val5 = ClassHasher<HandsEquipmentSet>.GetHash128(handsEquipmentSets[j]);
				result.Append(ref val5);
			}
		}
		List<WeaponSlot> additionalLimbs = m_AdditionalLimbs;
		if (additionalLimbs != null)
		{
			for (int k = 0; k < additionalLimbs.Count; k++)
			{
				Hash128 val6 = ClassHasher<WeaponSlot>.GetHash128(additionalLimbs[k]);
				result.Append(ref val6);
			}
		}
		Hash128 val7 = ClassHasher<HandsEquipmentSet>.GetHash128(m_PolymorphHandsEquipmentSet);
		result.Append(ref val7);
		List<WeaponSlot> polymorphAdditionalLimbs = m_PolymorphAdditionalLimbs;
		if (polymorphAdditionalLimbs != null)
		{
			for (int l = 0; l < polymorphAdditionalLimbs.Count; l++)
			{
				Hash128 val8 = ClassHasher<WeaponSlot>.GetHash128(polymorphAdditionalLimbs[l]);
				result.Append(ref val8);
			}
		}
		result.Append(ref m_PolymorphKeepSlots);
		UsableSlot[] quickSlots = QuickSlots;
		if (quickSlots != null)
		{
			for (int m = 0; m < quickSlots.Length; m++)
			{
				Hash128 val9 = ClassHasher<UsableSlot>.GetHash128(quickSlots[m]);
				result.Append(ref val9);
			}
		}
		Hash128 val10 = ClassHasher<ArmorSlot>.GetHash128(Armor);
		result.Append(ref val10);
		Hash128 val11 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentShirt>>.GetHash128(Shirt);
		result.Append(ref val11);
		Hash128 val12 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentBelt>>.GetHash128(Belt);
		result.Append(ref val12);
		Hash128 val13 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentHead>>.GetHash128(Head);
		result.Append(ref val13);
		Hash128 val14 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentGlasses>>.GetHash128(Glasses);
		result.Append(ref val14);
		Hash128 val15 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentFeet>>.GetHash128(Feet);
		result.Append(ref val15);
		Hash128 val16 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentGloves>>.GetHash128(Gloves);
		result.Append(ref val16);
		Hash128 val17 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentNeck>>.GetHash128(Neck);
		result.Append(ref val17);
		Hash128 val18 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentRing>>.GetHash128(Ring1);
		result.Append(ref val18);
		Hash128 val19 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentRing>>.GetHash128(Ring2);
		result.Append(ref val19);
		Hash128 val20 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentWrist>>.GetHash128(Wrist);
		result.Append(ref val20);
		Hash128 val21 = ClassHasher<EquipmentSlot<BlueprintItemEquipmentShoulders>>.GetHash128(Shoulders);
		result.Append(ref val21);
		List<EquipmentSlot<BlueprintItemMechadendrite>> mechadendrites = Mechadendrites;
		if (mechadendrites != null)
		{
			for (int n = 0; n < mechadendrites.Count; n++)
			{
				Hash128 val22 = ClassHasher<EquipmentSlot<BlueprintItemMechadendrite>>.GetHash128(mechadendrites[n]);
				result.Append(ref val22);
			}
		}
		return result;
	}
}
