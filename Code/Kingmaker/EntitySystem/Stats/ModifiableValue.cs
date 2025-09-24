using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

[JsonObject(IsReference = true)]
public class ModifiableValue : IHashable
{
	public enum StackMode
	{
		Default,
		ForceStack,
		ForceNonStack
	}

	[JsonObject]
	public class Modifier : IEquatable<Modifier>
	{
		private static class ModValueRelationalComparer
		{
			public static int Compare(Modifier x, Modifier y)
			{
				if (x == y)
				{
					return 0;
				}
				if (y == null)
				{
					return 1;
				}
				return x?.ModValue.CompareTo(y.ModValue) ?? (-1);
			}
		}

		public ModifiableValue AppliedTo;

		public ModifierDescriptor ModDescriptor;

		public StackMode StackMode;

		public int ModValue;

		[CanBeNull]
		public EntityFact SourceFact;

		[CanBeNull]
		public string SourceComponent;

		[CanBeNull]
		public ItemEntity SourceItem;

		public StatType SourceStat;

		[NotNull]
		public static Comparison<Modifier> ValueComparer { get; } = ModValueRelationalComparer.Compare;


		public bool Stacks
		{
			get
			{
				if (StackMode != 0)
				{
					return StackMode == StackMode.ForceStack;
				}
				return ModDescriptor.IsStackable();
			}
		}

		public bool Remove()
		{
			if (AppliedTo == null)
			{
				return false;
			}
			return AppliedTo.RemoveModifier(this);
		}

		public override string ToString()
		{
			return $"{ModDescriptor}: {ModValue}";
		}

		public bool IsPermanent()
		{
			return ModDescriptor.IsPermanentModifier();
		}

		public bool IsRacial()
		{
			return ModDescriptor == ModifierDescriptor.Racial;
		}

		public bool Equals(Modifier other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (((AppliedTo == null && other.AppliedTo == null) || (AppliedTo != null && AppliedTo.Type == other.AppliedTo.Type && AppliedTo.Owner == other.AppliedTo.Owner)) && ModDescriptor == other.ModDescriptor && StackMode == other.StackMode && ModValue == other.ModValue && object.Equals(SourceFact, other.SourceFact) && SourceComponent == other.SourceComponent)
			{
				return object.Equals(SourceItem, other.SourceItem);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj != null)
			{
				if (this != obj)
				{
					if (obj.GetType() == GetType())
					{
						return Equals((Modifier)obj);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((((((((((uint)(((AppliedTo != null) ? AppliedTo.GetHashCode() : 0) * 397) ^ (uint)ModDescriptor) * 397) ^ (uint)StackMode) * 397) ^ (uint)ModValue) * 397) ^ (uint)((SourceFact != null) ? SourceFact.GetHashCode() : 0)) * 397) ^ (uint)((SourceComponent != null) ? SourceComponent.GetHashCode() : 0)) * 397) ^ ((SourceItem != null) ? SourceItem.GetHashCode() : 0);
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private int m_BaseValue;

	private readonly Dictionary<ModifierDescriptor, List<Modifier>> m_ModifierList = new Dictionary<ModifierDescriptor, List<Modifier>>();

	private StatsContainer m_Container;

	private List<ModifiableValue> m_Dependents;

	private List<EntityFact> m_DependentFacts;

	private List<EntityFactComponent> m_DependentComponents;

	private bool m_UpdateInternalModifiers;

	private bool m_UpdateDependentFacts;

	public readonly CountableFlag MoraleBonusesDisabled = new CountableFlag();

	public readonly CountableFlag InsightBonusesDisabled = new CountableFlag();

	public static readonly Func<Modifier, bool> FilterIsPermanent = (Modifier m) => m.IsPermanent();

	public StatType OriginalType { get; private set; }

	public StatType Type { get; private set; }

	public int ModifiedValue { get; private set; }

	public int ModifiedValueRaw { get; private set; }

	public int PermanentValue { get; private set; }

	public int BaseValue
	{
		get
		{
			return m_BaseValue;
		}
		set
		{
			if (value != m_BaseValue)
			{
				m_BaseValue = Math.Max(MinValue, value);
				UpdateValue();
			}
		}
	}

	protected StatsContainer Container => m_Container;

	public Entity Owner => m_Container?.Owner;

	protected virtual int MinValue => int.MinValue;

	protected virtual bool IgnoreModifiers => false;

	public IEnumerable<Modifier> Modifiers => m_ModifierList.SelectMany((KeyValuePair<ModifierDescriptor, List<Modifier>> i) => i.Value);

	public event Action<ModifiableValue, int> OnChanged;

	public void Initialize(StatsContainer container, StatType type)
	{
		m_Container = container;
		OriginalType = type;
		Type = type.TryGetOverride(Container.Owner);
		try
		{
			OnInitialize();
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
		}
	}

	protected virtual void OnInitialize()
	{
	}

	public void PrePostLoad(StatsContainer container, StatType type)
	{
		Initialize(container, type);
		int num = (PermanentValue = BaseValue);
		int modifiedValue = (ModifiedValueRaw = num);
		ModifiedValue = modifiedValue;
	}

	public void PostLoad()
	{
		try
		{
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual int CalculatePermanentValue()
	{
		return ApplyModifiersFiltered(CalculateBaseValue(BaseValue), FilterIsPermanent);
	}

	protected virtual int CalculateBaseValue(int baseValue)
	{
		return baseValue;
	}

	public void MoraleDisable()
	{
		MoraleBonusesDisabled.Retain();
		UpdateValue();
	}

	public void MoraleEnable()
	{
		MoraleBonusesDisabled.Release();
		UpdateValue();
	}

	public void InsightDisable()
	{
		InsightBonusesDisabled.Retain();
		UpdateValue();
	}

	public void InsightEnable()
	{
		InsightBonusesDisabled.Release();
		UpdateValue();
	}

	public void AddDependentValue(ModifiableValue dependent)
	{
		if (m_Dependents == null)
		{
			m_Dependents = new List<ModifiableValue>();
		}
		if (!m_Dependents.HasItem(dependent))
		{
			m_Dependents.Add(dependent);
		}
	}

	public void RemoveDependentValue(ModifiableValue dependent)
	{
		if (m_Dependents == null || !m_Dependents.Remove(dependent))
		{
			PFLog.Default.Error("Error in RemoveDependentValue");
		}
	}

	private Modifier AddModifier(Modifier mod)
	{
		ModifierDescriptor modDescriptor = mod.ModDescriptor;
		List<Modifier> list = m_ModifierList.Get(modDescriptor);
		if (list == null)
		{
			m_ModifierList.Add(modDescriptor, list = new List<Modifier>());
		}
		List<Modifier> list2 = null;
		foreach (Modifier item in list)
		{
			if (item.SourceFact == mod.SourceFact && item.SourceComponent == mod.SourceComponent && item.ModValue == mod.ModValue && item.ModDescriptor == mod.ModDescriptor)
			{
				if (list2 == null)
				{
					list2 = TempList.Get<Modifier>();
				}
				list2.Add(item);
			}
		}
		if (list2 != null)
		{
			foreach (Modifier item2 in list2)
			{
				list.Remove(item2);
			}
			PFLog.Default.Error("ModifiableValue.AddModifier: remove duplicated modifier for {0}.{1}. {2}, {3}, {4}, {5}", Owner, Type, mod.ModValue, mod.ModDescriptor, mod.SourceFact, mod.SourceComponent);
		}
		list.Add(mod);
		list.Sort(Modifier.ValueComparer);
		if (!IgnoreModifiers)
		{
			UpdateValue(mod.SourceFact);
		}
		mod.AppliedTo = this;
		if (!IgnoreModifiers)
		{
			using (ProfileScope.New("HandleModifierAdded"))
			{
				Owner?.GetOptional<UnitPartNonStackBonuses>()?.HandleModifierAdded(this, mod);
			}
		}
		return mod;
	}

	public IEnumerable<Modifier> GetDisplayModifiers()
	{
		ModifierDescriptor[] sortedValues = ModifierDescriptorComparer.SortedValues;
		foreach (ModifierDescriptor key in sortedValues)
		{
			List<Modifier> list = m_ModifierList.Get(key);
			if (list == null || list.Count <= 0)
			{
				continue;
			}
			Modifier penalty = null;
			Modifier bonus = null;
			foreach (Modifier item in list)
			{
				if (item.Stacks)
				{
					yield return item;
				}
				else if (item.ModValue > 0)
				{
					if (bonus == null || bonus.ModValue < item.ModValue)
					{
						bonus = item;
					}
				}
				else if (penalty == null || penalty.ModValue > item.ModValue)
				{
					penalty = item;
				}
			}
			if (bonus != null)
			{
				yield return bonus;
			}
			if (penalty != null)
			{
				yield return penalty;
			}
		}
	}

	private int GetStackingPenalty(ModifierDescriptor descriptor)
	{
		List<Modifier> list = m_ModifierList.Get(descriptor);
		if (list == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Modifier item in list)
		{
			if (item.Stacks && item.ModValue < num)
			{
				num = item.ModValue;
			}
		}
		return num;
	}

	public bool RemoveModifier(Modifier mod)
	{
		ModifierDescriptor modDescriptor = mod.ModDescriptor;
		if (!m_ModifierList.TryGetValue(modDescriptor, out var value))
		{
			return false;
		}
		if (value.Remove(mod))
		{
			PrepareForRemoval(mod);
			UpdateValue(mod.SourceFact);
			return true;
		}
		return false;
	}

	private void PrepareForRemoval(Modifier mod)
	{
		using (ProfileScope.New("Removing modifier"))
		{
			Owner?.GetOptional<UnitPartNonStackBonuses>()?.HandleModifierRemoving(this, mod);
		}
		mod.AppliedTo = null;
	}

	public void RemoveModifiers(ModifierDescriptor descriptor)
	{
		if (m_ModifierList.TryGetValue(descriptor, out var value))
		{
			m_ModifierList.Remove(descriptor);
			value.ForEach(PrepareForRemoval);
			UpdateValue();
		}
	}

	public void RemoveAllModifiers()
	{
		foreach (KeyValuePair<ModifierDescriptor, List<Modifier>> modifier in m_ModifierList)
		{
			modifier.Value.ForEach(PrepareForRemoval);
		}
		m_ModifierList.Clear();
		UpdateValue();
	}

	public void RemoveModifiersFrom(EntityFactComponent source)
	{
		foreach (KeyValuePair<ModifierDescriptor, List<Modifier>> modifier in m_ModifierList)
		{
			modifier.Value.RemoveAll(delegate(Modifier m)
			{
				if (m.SourceFact == source.Fact && m.SourceComponent == source.SourceBlueprintComponentName)
				{
					PrepareForRemoval(m);
					return true;
				}
				return false;
			});
		}
		UpdateValue(source.Fact);
	}

	public bool ContainsModifier(ModifierDescriptor descriptor)
	{
		return m_ModifierList.ContainsKey(descriptor);
	}

	public ReadonlyList<Modifier> GetModifiers(ModifierDescriptor descriptor)
	{
		return m_ModifierList.Get(descriptor);
	}

	[CanBeNull]
	public Modifier AddModifier(int value, [NotNull] EntityFactComponent source, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default,
			SourceFact = source.Fact,
			SourceComponent = source.SourceBlueprintComponentName
		});
	}

	[CanBeNull]
	public Modifier AddItemModifier(int value, [NotNull] ItemEntity itemSource, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default,
			SourceItem = itemSource
		});
	}

	[CanBeNull]
	public Modifier AddItemModifier(int value, [NotNull] EntityFactComponent source, [NotNull] ItemEntity itemSource, ModifierDescriptor desc = ModifierDescriptor.None)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default,
			SourceFact = source.Fact,
			SourceComponent = source.SourceBlueprintComponentName,
			SourceItem = itemSource
		});
	}

	[CanBeNull]
	public Modifier AddModifier(int value, StatType sourceStat, ModifierDescriptor desc)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default,
			SourceStat = sourceStat
		});
	}

	[CanBeNull]
	protected Modifier AddInternalModifier(int value, StatType sourceStat, ModifierDescriptor desc)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default,
			SourceStat = sourceStat
		});
	}

	[CanBeNull]
	protected Modifier AddInternalModifier(int value, ModifierDescriptor desc)
	{
		return AddModifier(new Modifier
		{
			ModValue = value,
			ModDescriptor = desc,
			StackMode = StackMode.Default
		});
	}

	public void UpdateValue(EntityFact reason = null)
	{
		if (m_UpdateInternalModifiers)
		{
			return;
		}
		int modifiedValue = ModifiedValue;
		m_UpdateInternalModifiers = true;
		UpdateInternalModifiers();
		m_UpdateInternalModifiers = false;
		ModifiedValueRaw = ApplyModifiersFiltered(CalculateBaseValue(BaseValue), null);
		UnitPartStatsOverride optional = Owner.GetOptional<UnitPartStatsOverride>();
		if (optional != null && optional.TryGetOverride(Type, out var value))
		{
			ModifiedValue = value;
		}
		else
		{
			ModifiedValue = Math.Max(MinValue, ModifiedValueRaw);
		}
		PermanentValue = CalculatePermanentValue();
		OnUpdate();
		if (m_UpdateDependentFacts)
		{
			return;
		}
		UpdateDependentFactsAndComponents(reason);
		if (modifiedValue != ModifiedValue)
		{
			this.OnChanged?.Invoke(this, modifiedValue);
		}
		if (m_Dependents == null)
		{
			return;
		}
		foreach (ModifiableValue item in m_Dependents.ToTempList())
		{
			item.UpdateValue();
		}
	}

	protected virtual void OnUpdate()
	{
	}

	public void AddDependentFact(EntityFact fact)
	{
		if (m_DependentFacts == null)
		{
			m_DependentFacts = new List<EntityFact>();
		}
		if (!m_DependentFacts.HasItem(fact))
		{
			m_DependentFacts.Add(fact);
		}
	}

	public void RemoveDependentFact(EntityFact fact)
	{
		m_DependentFacts?.Remove(fact);
	}

	public void AddDependentComponent(EntityFactComponent component)
	{
		if (m_DependentComponents == null)
		{
			m_DependentComponents = new List<EntityFactComponent>();
		}
		if (!m_DependentComponents.HasItem(component))
		{
			m_DependentComponents.Add(component);
		}
	}

	public void RemoveDependentComponent(EntityFactComponent component)
	{
		m_DependentComponents?.Remove(component);
	}

	public int GetDescriptorBonus(ModifierDescriptor descriptor)
	{
		return ApplyModifiersFiltered(0, (Modifier m) => m.ModDescriptor == descriptor);
	}

	private void UpdateDependentFactsAndComponents(EntityFact reason = null)
	{
		Entity owner = Owner;
		if ((owner != null && owner.IsDisposingNow) || m_UpdateDependentFacts)
		{
			return;
		}
		m_UpdateDependentFacts = true;
		if (m_DependentFacts != null)
		{
			foreach (EntityFact item in m_DependentFacts.ToList())
			{
				if (item != reason)
				{
					try
					{
						item.Reapply();
					}
					catch (Exception ex)
					{
						PFLog.Default.Exception(ex);
					}
				}
			}
		}
		if (m_DependentComponents != null)
		{
			foreach (EntityFactComponent item2 in m_DependentComponents.ToList())
			{
				if (item2 == null)
				{
					continue;
				}
				EntityFact fact = item2.Fact;
				if (fact != null && fact.Active && item2.Fact != reason)
				{
					try
					{
						item2.Recalculate();
					}
					catch (Exception ex2)
					{
						PFLog.Default.Exception(ex2);
					}
				}
			}
		}
		m_UpdateDependentFacts = false;
	}

	public void CleanupModifiers()
	{
		bool flag = false;
		foreach (KeyValuePair<ModifierDescriptor, List<Modifier>> modifier4 in m_ModifierList)
		{
			int num = modifier4.Value.RemoveAll(delegate(Modifier m)
			{
				if (!IsModifierValid(Owner, m))
				{
					PrepareForRemoval(m);
					return true;
				}
				return false;
			});
			flag = flag || num > 0;
		}
		foreach (KeyValuePair<ModifierDescriptor, List<Modifier>> modifier5 in m_ModifierList)
		{
			for (int j = 0; j < modifier5.Value.Count; j++)
			{
				Modifier modifier = modifier5.Value[j];
				for (int k = j + 1; k < modifier5.Value.Count; k++)
				{
					Modifier modifier2 = modifier5.Value[k];
					if (modifier2?.SourceFact != null && modifier?.SourceFact == modifier2.SourceFact && modifier?.ModValue == modifier2.ModValue && modifier.SourceComponent == modifier2.SourceComponent)
					{
						PrepareForRemoval(modifier);
						PrepareForRemoval(modifier2);
						List<Modifier> value = modifier5.Value;
						int index = k;
						Modifier value2 = (modifier5.Value[j] = null);
						value[index] = value2;
					}
				}
			}
			flag |= modifier5.Value.RemoveAll((Modifier i) => i == null) > 0;
		}
		if (flag)
		{
			UpdateValue();
		}
	}

	private static bool IsModifierValid(Entity owner, Modifier mod)
	{
		ItemEntity sourceItem = mod.SourceItem;
		PartInventory optional = owner.GetOptional<PartInventory>();
		if (sourceItem != null && sourceItem.Collection != optional?.Collection)
		{
			return false;
		}
		if (mod.SourceFact == null)
		{
			return true;
		}
		if (!owner.Facts.Contains(mod.SourceFact))
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(mod.SourceComponent))
		{
			return true;
		}
		return mod.SourceFact.SelectComponents((BlueprintComponent c) => c.name == mod.SourceComponent).Any();
	}

	protected virtual void UpdateInternalModifiers()
	{
	}

	protected int ApplyModifiersFiltered(int baseValue, Func<Modifier, bool> filter)
	{
		int num = baseValue;
		if (IgnoreModifiers)
		{
			return num;
		}
		foreach (KeyValuePair<ModifierDescriptor, List<Modifier>> modifier in m_ModifierList)
		{
			List<Modifier> value = modifier.Value;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			foreach (Modifier item in value)
			{
				if (filter == null || filter(item))
				{
					if (item.Stacks)
					{
						num2 += item.ModValue;
					}
					if (!item.Stacks && item.ModValue > num3)
					{
						num3 = item.ModValue;
					}
					if (!item.Stacks && item.ModValue < num4)
					{
						num4 = item.ModValue;
					}
				}
			}
			num += num2 + num3 + num4;
		}
		return num;
	}

	public static implicit operator int([CanBeNull] ModifiableValue v)
	{
		return v?.ModifiedValue ?? 0;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_BaseValue);
		return result;
	}
}
