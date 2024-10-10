using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

[JsonObject(MemberSerialization.OptIn)]
public abstract class ItemEntity : MechanicEntity<BlueprintItem>, IUIDataProvider, IItemEntity, IMechanicEntity, IEntity, IDisposable, IHashable
{
	public class ContextData : ContextData<ContextData>
	{
		public ItemEntity Item { get; private set; }

		public ContextData Setup(ItemEntity item)
		{
			Item = item;
			return this;
		}

		protected override void Reset()
		{
			Item = null;
		}
	}

	public class IdentifyRollData : IHashable
	{
		[JsonProperty]
		public UnitReference Identifier;

		[JsonProperty]
		public int SkillValue;

		[JsonProperty]
		public bool UsedSpell;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			UnitReference obj = Identifier;
			Hash128 val = UnitReferenceHasher.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref SkillValue);
			result.Append(ref UsedSpell);
			return result;
		}
	}

	private static readonly BitArray SortedIndices = new BitArray(500);

	[JsonProperty]
	private int m_Count = 1;

	[JsonProperty]
	[GameStateIgnore]
	private int m_InventorySlotIndex;

	[CanBeNull]
	private ItemEnchantmentCollection m_Enchantments;

	[JsonProperty]
	[CanBeNull]
	private EntityFact[] m_FactsAppliedToWielder;

	[JsonProperty]
	public List<BlueprintItemEnchantment> RemovedEnchantments = new List<BlueprintItemEnchantment>();

	[JsonProperty]
	private bool m_SkinningSuccessful;

	[JsonProperty]
	private EntityRef<MechanicEntity> m_WielderRef;

	[JsonProperty]
	private readonly List<IdentifyRollData> m_IdentifyRolls = new List<IdentifyRollData>();

	[JsonProperty]
	private bool m_NotLootable;

	private int m_EnchantmentsRuntimeVersion;

	private readonly List<ItemEnchantment> m_CachedEnchantments = new List<ItemEnchantment>();

	[JsonProperty]
	public TimeSpan Time { get; set; }

	[JsonProperty]
	[NotNull]
	public List<Ability> Abilities { get; } = new List<Ability>();


	[JsonProperty]
	public int Charges { get; set; }

	[JsonProperty]
	public bool IsIdentified { get; private set; }

	[JsonProperty]
	public TimeSpan? SellTime { get; set; }

	[JsonProperty]
	[CanBeNull]
	public BlueprintArea OriginArea { get; private set; }

	[JsonProperty]
	[CanBeNull]
	[Obsolete("Do not ever use this, use VendorBlueprint")]
	public BaseUnitEntity Vendor { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public BlueprintMechanicEntityFact VendorBlueprint { get; private set; }

	[JsonProperty(PropertyName = "Collection", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[GameStateIgnore]
	private ItemsCollection CollectionConverter
	{
		get
		{
			return null;
		}
		set
		{
			Collection = value ?? Collection;
		}
	}

	public ItemsCollection Collection { get; set; }

	public ItemSlot HoldingSlot { get; set; }

	[CanBeNull]
	public MechanicEntity Wielder => m_WielderRef.Entity;

	public override SceneEntitiesState HoldingState
	{
		get
		{
			object obj = Owner?.HoldingState;
			if (obj == null)
			{
				ItemsCollection collection = Collection;
				if (collection == null)
				{
					return null;
				}
				Entity concreteOwner = collection.ConcreteOwner;
				if (concreteOwner == null)
				{
					return null;
				}
				obj = concreteOwner.HoldingState;
			}
			return (SceneEntitiesState)obj;
		}
	}

	public override bool IsPreview => Owner?.IsPreview ?? false;

	public bool HasUniqueOriginArea
	{
		get
		{
			if (m_Count <= 1)
			{
				return OriginArea != null;
			}
			return false;
		}
	}

	public bool HasUniqueVendor
	{
		get
		{
			if (m_Count <= 1)
			{
				return VendorBlueprint != null;
			}
			return false;
		}
	}

	public bool HasUniqueSourceDescription
	{
		get
		{
			if (!IsStackable)
			{
				if (!HasUniqueVendor)
				{
					return HasUniqueOriginArea;
				}
				return true;
			}
			return false;
		}
	}

	public string OriginAreaDescription
	{
		get
		{
			if (OriginArea != null)
			{
				return string.Format(UIConfig.Instance.ItemOriginOwnerDescription, OriginArea.AreaName.Text);
			}
			return null;
		}
	}

	private string VendorName
	{
		get
		{
			if (!(VendorBlueprint is BlueprintUnit blueprintUnit))
			{
				return VendorBlueprint?.Name;
			}
			return blueprintUnit.CharacterName;
		}
	}

	public string VendorDescription
	{
		get
		{
			if (VendorBlueprint != null)
			{
				return string.Format(UIConfig.Instance.ItemVendorDescription, VendorName);
			}
			return null;
		}
	}

	public string UniqueSourceDescription => VendorDescription ?? OriginAreaDescription;

	public int Count => m_Count;

	public bool IsInStash => HoldingSlot == null;

	public ItemsItemOrigin Origin => base.Blueprint.Origin;

	[CanBeNull]
	public MechanicEntity Owner => HoldingSlot?.Owner;

	public virtual bool CanBeAssembled => false;

	public virtual bool CanBeDisassembled => false;

	protected virtual bool RemoveFromSlotWhenNoCharges => false;

	public bool ToCargoAutomatically
	{
		get
		{
			if (!base.Blueprint.ToCargoAutomatically)
			{
				return Game.Instance.Player.ItemsToCargo.Contains(base.Blueprint);
			}
			return true;
		}
	}

	[NotNull]
	public List<ItemEnchantment> Enchantments
	{
		get
		{
			UpdateCachedEnchantments(m_CachedEnchantments);
			return m_CachedEnchantments;
		}
	}

	public virtual int EnchantmentValue => EnchantmentsCollection?.EnchantmentValue ?? 0;

	[CanBeNull]
	public ItemEnchantmentCollection EnchantmentsCollection => m_Enchantments;

	public virtual bool IsLootable
	{
		get
		{
			if (!m_NotLootable)
			{
				return !(base.Blueprint is BlueprintItemEquipment blueprintItemEquipment) || !blueprintItemEquipment.IsUnlootable;
			}
			return false;
		}
	}

	public int InventorySlotIndex => UpdateSlotIndex();

	public bool IsSpendCharges => (base.Blueprint as BlueprintItemEquipment)?.SpendCharges ?? false;

	public float TotalWeight => (float)Count * base.Blueprint.Weight;

	public bool IsStackable
	{
		get
		{
			if (!base.Blueprint.IsActuallyStackable)
			{
				return Collection?.ForceStackable ?? false;
			}
			return true;
		}
	}

	public virtual bool InstantiateEnchantments => true;

	public bool IsUsableFromInventory
	{
		get
		{
			if (base.Blueprint is BlueprintItemEquipmentUsable)
			{
				return !Game.Instance.Player.IsInCombat;
			}
			return false;
		}
	}

	public virtual bool IsPartOfAnotherItem => false;

	public new string Name
	{
		get
		{
			if (!IsIdentified)
			{
				return "";
			}
			return base.Blueprint.Name;
		}
	}

	public string Description
	{
		get
		{
			if (!IsIdentified)
			{
				return "";
			}
			return base.Blueprint.Description;
		}
	}

	public string FlavorText
	{
		get
		{
			if (!IsIdentified)
			{
				return "";
			}
			return base.Blueprint.FlavorText;
		}
	}

	public Sprite Icon => base.Blueprint.Icon;

	public string NameForAcronym => base.Blueprint.NameForAcronym;

	public float ProfitFactorCost
	{
		get
		{
			double num = this.GetProfitFactorCost();
			if (IsSpendCharges)
			{
				BlueprintItemEquipmentUsable blueprintItemEquipmentUsable = base.Blueprint as BlueprintItemEquipmentUsable;
				if ((bool)blueprintItemEquipmentUsable)
				{
					num *= (double)Charges / (double)blueprintItemEquipmentUsable.Charges;
				}
			}
			return (int)num;
		}
	}

	public double CostPF => this.GetCostPF();

	[JsonProperty]
	public bool IsNonRemovable { get; set; }

	public virtual bool Assemble()
	{
		return false;
	}

	public virtual bool Disassemble()
	{
		return false;
	}

	public void SetOriginAreaIfNull(BlueprintArea area)
	{
		if (OriginArea == null)
		{
			OriginArea = area;
		}
	}

	public void SetVendorIfNull(MechanicEntity vendor)
	{
		if (VendorBlueprint == null)
		{
			VendorBlueprint = vendor.Blueprint;
		}
	}

	protected ItemEntity(BlueprintItem blueprint)
		: base(Uuid.Instance.CreateString(), isInGame: true, blueprint)
	{
	}

	protected ItemEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		ApplyEnchantments(onInitializeOrEquip: true);
		m_InventorySlotIndex = -1;
		BlueprintItemEquipment blueprintItemEquipment = base.Blueprint as BlueprintItemEquipment;
		if ((bool)blueprintItemEquipment && blueprintItemEquipment.GainAbility && blueprintItemEquipment.SpendCharges)
		{
			Charges = blueprintItemEquipment.Charges;
		}
		IsNonRemovable = SimpleBlueprintExtendAsObject.Or(blueprintItemEquipment, null)?.IsNonRemovable ?? false;
		IsIdentified = base.OriginalBlueprint.IdentifyDC == 0;
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return new ItemFact((BlueprintItem)blueprint);
	}

	public virtual bool CanBeMerged(ItemEntity other)
	{
		if (other != null && IsStackable && base.Blueprint == other.Blueprint && Charges == other.Charges)
		{
			return HoldingSlot == null;
		}
		return false;
	}

	public bool TryMerge(ItemEntity other)
	{
		if (other.Collection != null && other.Collection != Collection)
		{
			PFLog.Default.Error("Can't merge items from different collections");
			return false;
		}
		if (!CanBeMerged(other))
		{
			return false;
		}
		IncrementCount(other.Count);
		other.DecrementCount(other.Count, Collection?.ForceStackable ?? false);
		other.Collection?.Extract(other);
		other.Dispose();
		return true;
	}

	public ItemEntity TryMergeInCollection()
	{
		if (Collection == null || !IsStackable)
		{
			return this;
		}
		foreach (ItemEntity item in Collection.Items)
		{
			if (item != this && item.TryMerge(this))
			{
				return item;
			}
		}
		return this;
	}

	public void IncrementCount(int value, bool force = false)
	{
		m_Count += value;
		Collection?.DeltaWeight(base.Blueprint.Weight * (float)value);
	}

	public void DecrementCount(int value, bool force = false)
	{
		int num = Math.Min(Count, value);
		m_Count = Math.Max(0, Count - value);
		Collection?.DeltaWeight((0f - base.Blueprint.Weight) * (float)num);
	}

	public void SetCount(int value)
	{
		m_Count = Math.Max(0, value);
	}

	private void ApplyEnchantments(bool onInitializeOrEquip)
	{
		if (!InstantiateEnchantments)
		{
			return;
		}
		ItemEnchantmentCollection itemEnchantmentCollection = (m_Enchantments = Facts.EnsureFactProcessor<ItemEnchantmentCollection>());
		List<ItemEnchantment> list = null;
		List<BlueprintItemEnchantment> list2 = null;
		if (!onInitializeOrEquip)
		{
			foreach (ItemEnchantment rawFact in itemEnchantmentCollection.RawFacts)
			{
				if (rawFact.ParentContext == null && !base.Blueprint.Enchantments.HasItem(rawFact.Blueprint))
				{
					list = list ?? TempList.Get<ItemEnchantment>();
					list.Add(rawFact);
					PFLog.Default.Warning(base.Blueprint, $"{this}: remove redunant enchantment '{rawFact}' (owner: {Owner ?? Collection?.OwnerUnit})");
				}
			}
		}
		foreach (BlueprintItemEnchantment enchantment in base.Blueprint.Enchantments)
		{
			if (!ContainsEnchantmentFromBlueprint(itemEnchantmentCollection, enchantment) && !RemovedEnchantments.Contains(enchantment))
			{
				list2 = list2 ?? TempList.Get<BlueprintItemEnchantment>();
				list2.Add(enchantment);
				if (!onInitializeOrEquip)
				{
					PFLog.Default.Warning(base.Blueprint, $"{this}: restore missing enchantment '{enchantment}' (owner: {Owner ?? Collection?.OwnerUnit})");
				}
			}
		}
		if (list != null)
		{
			foreach (ItemEnchantment item in list)
			{
				itemEnchantmentCollection.Remove(item);
			}
		}
		if (!list2.Any())
		{
			return;
		}
		foreach (BlueprintItemEnchantment item2 in list2)
		{
			AddEnchantmentInternal(item2);
		}
	}

	private static bool ContainsEnchantmentFromBlueprint([NotNull] ItemEnchantmentCollection collection, BlueprintItemEnchantment enchantment)
	{
		foreach (ItemEnchantment rawFact in collection.RawFacts)
		{
			if (rawFact.Blueprint == enchantment && rawFact.ParentContext == null)
			{
				return true;
			}
		}
		return false;
	}

	public ItemEnchantment AddEnchantment([NotNull] BlueprintItemEnchantment blueprint, [NotNull] MechanicsContext parentContext, Rounds? duration = null)
	{
		return AddEnchantmentInternal(blueprint, parentContext, duration);
	}

	public virtual void RemoveEnchantment(ItemEnchantment enchantment)
	{
		EnchantmentsCollection?.Remove(enchantment);
	}

	private ItemEnchantment AddEnchantmentInternal(BlueprintItemEnchantment blueprint, MechanicsContext parentContext = null, Rounds? duration = null)
	{
		return m_Enchantments?.AddEnchantment(blueprint, parentContext, duration);
	}

	protected virtual void UpdateCachedEnchantments(List<ItemEnchantment> enchantmentsList)
	{
		if (m_Enchantments != null && m_Enchantments.RuntimeVersion != m_EnchantmentsRuntimeVersion)
		{
			enchantmentsList.Clear();
			enchantmentsList.AddRange(m_Enchantments.Enumerable);
			m_EnchantmentsRuntimeVersion = m_Enchantments.RuntimeVersion;
		}
	}

	[CanBeNull]
	public ItemEnchantment GetEnchantment(BlueprintItemEnchantment blueprint)
	{
		return Enchantments.GetFact(blueprint);
	}

	public bool HasEnchantment(BlueprintItemEnchantment blueprint)
	{
		return GetEnchantment(blueprint) != null;
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append(GetType().Name);
		builder.Append('[');
		builder.Append(base.Blueprint.name);
		if (Count != 1)
		{
			builder.Append('*');
			builder.Append(Count);
		}
		builder.Append("]#");
		builder.Append(base.UniqueId);
		if (Collection?.Owner != null)
		{
			builder.Append("##");
			builder.Append(Collection.Owner);
		}
		return builder.ToString();
	}

	public virtual ItemEntity Split(int count)
	{
		if (!IsStackable)
		{
			PFLog.Default.Error($"Can't split item {base.Blueprint}");
			return this;
		}
		if (Count < count)
		{
			PFLog.Default.Error($"Can't split item: required count {count}, items count {Count}");
			return this;
		}
		if (Count == count)
		{
			return this;
		}
		DecrementCount(count);
		ItemEntity itemEntity = base.Blueprint.CreateEntity();
		itemEntity.IncrementCount(count - 1, Collection?.ForceStackable ?? false);
		itemEntity.m_SkinningSuccessful = m_SkinningSuccessful;
		itemEntity.Time = Game.Instance.Player.GameTime;
		itemEntity.Charges = Charges;
		itemEntity.IsIdentified = IsIdentified;
		itemEntity.SellTime = SellTime;
		itemEntity.m_IdentifyRolls.AddRange(m_IdentifyRolls);
		Collection?.Insert(itemEntity);
		return itemEntity;
	}

	public virtual void OnDidEquipped([NotNull] MechanicEntity wielder)
	{
		m_WielderRef = wielder ?? throw new ArgumentNullException("wielder");
		ApplyEnchantments(onInitializeOrEquip: true);
		m_Enchantments?.SetActive(active: true);
		ReapplyFactsForWielder();
		EventBus.RaiseEvent((IItemEntity)this, (Action<IEquipItemHandler>)delegate(IEquipItemHandler x)
		{
			x.OnDidEquipped();
		}, isCheckRuntime: true);
	}

	public virtual void OnWillUnequip()
	{
		EventBus.RaiseEvent((IItemEntity)this, (Action<IEquipItemHandler>)delegate(IEquipItemHandler x)
		{
			x.OnWillUnequip();
		}, isCheckRuntime: true);
		if (Wielder != null)
		{
			Abilities.ForEach(delegate(Ability v)
			{
				Wielder.Facts.Remove(v);
			});
			EntityFact[] array = m_FactsAppliedToWielder.EmptyIfNull();
			foreach (EntityFact fact in array)
			{
				Wielder.Facts.Remove(fact);
			}
		}
		if (m_Enchantments != null)
		{
			ItemEnchantment[] array2 = m_Enchantments.Enumerable.Where((ItemEnchantment e) => e.RemoveOnUnequipItem).ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				m_Enchantments.Remove(array2[j]);
			}
			m_Enchantments.SetActive(active: false);
		}
		Abilities.Clear();
		m_FactsAppliedToWielder = null;
		m_WielderRef = null;
	}

	protected override void OnPostLoad()
	{
		if (InstantiateEnchantments)
		{
			m_Enchantments = Facts.EnsureFactProcessor<ItemEnchantmentCollection>();
		}
		if (Vendor != null)
		{
			VendorBlueprint = Vendor.Blueprint;
			Vendor = null;
		}
	}

	protected override void OnDispose()
	{
		ItemsCollection collection = Collection;
		if (collection != null && !collection.IsDisposingNow)
		{
			Collection.Remove(this);
		}
		base.OnDispose();
	}

	public void ReapplyFactsForWielder()
	{
		if (Wielder == null)
		{
			return;
		}
		Abilities.ForEach(delegate(Ability v)
		{
			Wielder.Facts.Remove(v);
		});
		Abilities.Clear();
		m_FactsAppliedToWielder?.ForEach(delegate(EntityFact i)
		{
			Wielder.Facts.Remove(i);
		});
		m_FactsAppliedToWielder = base.Blueprint.GetComponents<AddFactToEquipmentWielder>().Select(delegate(AddFactToEquipmentWielder c)
		{
			EntityFact entityFact = Wielder.AddFact(c.Fact);
			if (entityFact != null)
			{
				entityFact.AddSource(this);
				return entityFact;
			}
			return entityFact;
		}).NotNull()
			.ToArray();
		OnReapplyFactsForWielder();
	}

	protected virtual void OnReapplyFactsForWielder()
	{
	}

	public bool CanBeEquippedBy(MechanicEntity owner)
	{
		if (!owner.IsPlayerFaction || CanBeEquippedInternal(owner))
		{
			return true;
		}
		PartUnitBody optional = owner.GetOptional<PartUnitBody>();
		if (optional == null || !optional.IsInitializing)
		{
			return false;
		}
		PFLog.Default.Warning("'{0}' equip non-equippable item '{1}' during body initialization", owner.OriginalBlueprint, base.Blueprint);
		return true;
	}

	protected virtual bool CanBeEquippedInternal(MechanicEntity owner)
	{
		return (base.Blueprint as BlueprintItemEquipment)?.CanBeEquippedBy(owner) ?? false;
	}

	public void OnOpenDescriptionFirstTime()
	{
		EventBus.RaiseEvent((IItemEntity)this, (Action<IPlayerOpenItemDescriptionFirstTimeHandler>)delegate(IPlayerOpenItemDescriptionFirstTimeHandler h)
		{
			h.HandlePlayerOpenItemDescriptionFirstTime();
		}, isCheckRuntime: true);
	}

	public void OnOpenDescription()
	{
		Game.Instance.GameCommandQueue.AddCommand(new OpenItemDescriptionGameCommand(this));
	}

	public int UpdateSlotIndex(bool force = false)
	{
		if (force)
		{
			m_InventorySlotIndex = -1;
		}
		return UpdateSlotIndexInternal(null);
	}

	public int SetSlotIndex(int newIndex)
	{
		return UpdateSlotIndexInternal(newIndex);
	}

	public abstract bool OnPostLoadValidation();

	private int UpdateSlotIndexInternal(int? newIndex)
	{
		using (ProfileScope.New("ItemEntity.UpdateSlotIndex"))
		{
			try
			{
				if (!IsLootable && !newIndex.HasValue)
				{
					m_InventorySlotIndex = -1;
					return -1;
				}
				int inventorySlotIndex = m_InventorySlotIndex;
				if (newIndex.HasValue)
				{
					m_InventorySlotIndex = newIndex.Value;
				}
				bool flag = HoldingSlot?.Owner != null && (!HoldingSlot.Owner.IsDead || HoldingSlot.Owner.IsPlayerFaction);
				if (m_InventorySlotIndex >= 0 && (Collection == null || flag))
				{
					m_InventorySlotIndex = -1;
				}
				if (m_InventorySlotIndex < 0 && Collection != null && !flag)
				{
					m_InventorySlotIndex = -1;
					int num = 0;
					foreach (ItemEntity item in Collection.Items)
					{
						if (num < item.m_InventorySlotIndex)
						{
							num = item.m_InventorySlotIndex;
						}
					}
					SortedIndices.Length = num + 2;
					SortedIndices.SetAll(value: false);
					foreach (ItemEntity item2 in Collection.Items)
					{
						if (item2.m_InventorySlotIndex >= 0)
						{
							SortedIndices.Set(item2.m_InventorySlotIndex, value: true);
						}
					}
					for (int i = 0; i < SortedIndices.Length; i++)
					{
						if (!SortedIndices[i])
						{
							m_InventorySlotIndex = i;
							break;
						}
					}
					if (m_InventorySlotIndex < 0)
					{
						m_InventorySlotIndex = Collection.Items.Count;
					}
				}
				_ = m_InventorySlotIndex;
			}
			catch (Exception ex)
			{
				LogChannel.Default.Exception(ex);
			}
			return m_InventorySlotIndex;
		}
	}

	public void SetToCargoAutomatically(bool toCargo)
	{
		Game.Instance.GameCommandQueue.SetToCargoAutomatically(base.Blueprint, toCargo);
	}

	public bool SpendCharges()
	{
		return SpendCharges(Owner);
	}

	public bool SpendCharges(MechanicEntity user)
	{
		if (!(base.Blueprint is BlueprintItemEquipment { GainAbility: not false }))
		{
			PFLog.Default.Error(base.Blueprint, $"Item {base.Blueprint} doesn't gain ability");
			return false;
		}
		if (!IsSpendCharges)
		{
			return true;
		}
		bool flag = false;
		if (Charges > 0)
		{
			Charges--;
			EventBus.RaiseEvent(delegate(IItemChargesHandler h)
			{
				h.HandleItemChargeSpent(this);
			});
		}
		else
		{
			flag = true;
			PFLog.Default.Error("Has no charges");
		}
		if (Charges < 1 && RemoveFromSlotWhenNoCharges)
		{
			if (Count > 1)
			{
				DecrementCount(1);
				Charges = 1;
			}
			else
			{
				using (ContextData<ItemSlot.IgnoreLock>.Request())
				{
					Collection?.Remove(this);
				}
			}
		}
		return !flag;
	}

	public void RestoreCharges()
	{
		if (base.Blueprint is BlueprintItemEquipment { GainAbility: not false, SpendCharges: not false } blueprintItemEquipment)
		{
			Charges = blueprintItemEquipment.Charges;
		}
	}

	public void TryIdentify()
	{
		if (IsIdentified)
		{
			return;
		}
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.Player.Party;
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null && loadedAreaState.Settings.CapitalPartyMode)
		{
			enumerable = Game.Instance.Player.RemoteCompanions.Concat(new BaseUnitEntity[1] { Game.Instance.Player.MainCharacterEntity });
		}
		foreach (BaseUnitEntity item in enumerable)
		{
			TryIdentify(item.FromBaseUnitEntity(), 0);
		}
		if (!IsIdentified)
		{
			EventBus.RaiseEvent((IItemEntity)this, (Action<IIdentifyHandler>)delegate(IIdentifyHandler h)
			{
				h.OnFailedToIdentify();
			}, isCheckRuntime: true);
		}
	}

	public void TryIdentify(UnitReference character, int bonus)
	{
		if (!IsIdentified)
		{
			IAbstractUnitEntity entity = character.Entity;
			if (entity != null && !entity.ToBaseUnitEntity().LifeState.IsDead && character.Entity is UnitEntity initiator)
			{
				Rulebook.Trigger(new RulePerformIdentifyItem(initiator, this));
			}
		}
	}

	public void Identify()
	{
		IsIdentified = true;
		m_IdentifyRolls.Clear();
		(this as ItemEntityWeapon)?.Second?.Identify();
	}

	public IdentifyRollData GetIdentifyRollData(BaseUnitEntity unit)
	{
		IdentifyRollData identifyRollData = m_IdentifyRolls.FirstItem((IdentifyRollData d) => d.Identifier == unit);
		if (identifyRollData == null)
		{
			identifyRollData = new IdentifyRollData
			{
				Identifier = unit.FromBaseUnitEntity()
			};
			m_IdentifyRolls.Add(identifyRollData);
		}
		return identifyRollData;
	}

	public void MakeNotLootable()
	{
		m_NotLootable = true;
	}

	public bool IsAvailable()
	{
		if (base.Blueprint.Origin == ItemsItemOrigin.None)
		{
			return base.Blueprint.Rarity != BlueprintItem.ItemRarity.Unique;
		}
		return true;
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	[CanBeNull]
	public BaseUnitEntity GetBestAvailableUser()
	{
		if (!((base.Blueprint as BlueprintItemEquipment)?.Abilities.FirstOrDefault()))
		{
			return null;
		}
		BaseUnitEntity baseUnitEntity = (IsSuitableUnitForUseAbility(UIUtility.GetCurrentSelectedUnit()) ? UIUtility.GetCurrentSelectedUnit() : null);
		if (MeetRestrictions(baseUnitEntity))
		{
			return baseUnitEntity;
		}
		return null;
	}

	private bool MeetRestrictions(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return false;
		}
		BlueprintComponentsEnumerator<EquipmentRestriction> components = base.Blueprint.GetComponents<EquipmentRestriction>();
		if (components.Any())
		{
			foreach (EquipmentRestriction item in components)
			{
				if (!item.CanBeEquippedBy(unit))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool IsSuitableUnitForUseAbility(BaseUnitEntity unit)
	{
		if (unit != null && unit.State.CanAct && unit.Master == null)
		{
			UnitPartCompanion optional = unit.GetOptional<UnitPartCompanion>();
			if (optional == null || optional.State != CompanionState.ExCompanion)
			{
				return !unit.IsDetached;
			}
		}
		return false;
	}

	public bool TryUseFromInventory(BaseUnitEntity user, TargetWrapper target)
	{
		if (!IsUsableFromInventory)
		{
			PFLog.Default.Error($"Can't use item from inventory now: {this}");
			return false;
		}
		if (!IsSuitableUnitForUseAbility(user))
		{
			PFLog.Default.Error($"Invalid user: {user} (item: {this})");
			return false;
		}
		BlueprintAbility blueprintAbility = (base.Blueprint as BlueprintItemEquipment)?.Abilities.FirstOrDefault();
		if (!blueprintAbility)
		{
			PFLog.Default.Error($"Can't use item {this}");
			return false;
		}
		_ = blueprintAbility?.Range;
		Ability ability = user.Abilities.Add(blueprintAbility);
		if (ability == null)
		{
			PFLog.Default.Error($"Invalid ability blueprint: {blueprintAbility}");
			return false;
		}
		ability.AddSource(this);
		try
		{
			if (!ability.Data.IsAvailable)
			{
				PFLog.Default.Error($"Ability is not available: {ability}");
				return false;
			}
			if (!ability.Data.CanTarget(target, out var unavailableReason))
			{
				PFLog.Default.Error($"Invalid target for ability: {ability} (target; {target}) because of {unavailableReason}");
				return false;
			}
			RulePerformAbility rulePerformAbility = Rulebook.Trigger(new RulePerformAbility(ability, target));
			if (rulePerformAbility.Success)
			{
				rulePerformAbility.Result.InstantDeliver();
				rulePerformAbility.Result.Detach();
				int num = 0;
				while (!rulePerformAbility.Result.IsEnded && num++ < 1000)
				{
					rulePerformAbility.Result.Tick();
				}
				if (num >= 1000)
				{
					PFLog.Default.Error($"Hang up execution process when using {base.Blueprint} from inventory.");
				}
			}
			else
			{
				PFLog.Default.Error(ability.Blueprint, $"Spell casting failed: {ability}");
			}
			return true;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
		finally
		{
			user.Abilities.Remove(ability);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Count);
		EntityFact[] factsAppliedToWielder = m_FactsAppliedToWielder;
		if (factsAppliedToWielder != null)
		{
			for (int i = 0; i < factsAppliedToWielder.Length; i++)
			{
				Hash128 val2 = ClassHasher<EntityFact>.GetHash128(factsAppliedToWielder[i]);
				result.Append(ref val2);
			}
		}
		List<BlueprintItemEnchantment> removedEnchantments = RemovedEnchantments;
		if (removedEnchantments != null)
		{
			for (int j = 0; j < removedEnchantments.Count; j++)
			{
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(removedEnchantments[j]);
				result.Append(ref val3);
			}
		}
		result.Append(ref m_SkinningSuccessful);
		EntityRef<MechanicEntity> obj = m_WielderRef;
		Hash128 val4 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val4);
		List<IdentifyRollData> identifyRolls = m_IdentifyRolls;
		if (identifyRolls != null)
		{
			for (int k = 0; k < identifyRolls.Count; k++)
			{
				Hash128 val5 = ClassHasher<IdentifyRollData>.GetHash128(identifyRolls[k]);
				result.Append(ref val5);
			}
		}
		result.Append(ref m_NotLootable);
		TimeSpan val6 = Time;
		result.Append(ref val6);
		List<Ability> abilities = Abilities;
		if (abilities != null)
		{
			for (int l = 0; l < abilities.Count; l++)
			{
				Hash128 val7 = ClassHasher<Ability>.GetHash128(abilities[l]);
				result.Append(ref val7);
			}
		}
		int val8 = Charges;
		result.Append(ref val8);
		bool val9 = IsIdentified;
		result.Append(ref val9);
		if (SellTime.HasValue)
		{
			TimeSpan val10 = SellTime.Value;
			result.Append(ref val10);
		}
		Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(OriginArea);
		result.Append(ref val11);
		Hash128 val12 = ClassHasher<BaseUnitEntity>.GetHash128(Vendor);
		result.Append(ref val12);
		Hash128 val13 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(VendorBlueprint);
		result.Append(ref val13);
		bool val14 = IsNonRemovable;
		result.Append(ref val14);
		return result;
	}
}
public abstract class ItemEntity<TBlueprintItem> : ItemEntity, IHashable where TBlueprintItem : BlueprintItem
{
	public new TBlueprintItem Blueprint => (TBlueprintItem)base.Blueprint;

	public new TBlueprintItem OriginalBlueprint => (TBlueprintItem)base.OriginalBlueprint;

	public override Type RequiredBlueprintType => typeof(TBlueprintItem);

	public override bool OnPostLoadValidation()
	{
		return base.Blueprint as TBlueprintItem != null;
	}

	protected ItemEntity(TBlueprintItem bpItem)
		: base(bpItem)
	{
	}

	protected ItemEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
