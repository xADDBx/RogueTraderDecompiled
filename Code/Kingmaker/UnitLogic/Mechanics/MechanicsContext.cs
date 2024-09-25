using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public class MechanicsContext : IUIDataProvider, IHashable
{
	public class Data : ContextData<Data>
	{
		public MechanicsContext Context { get; private set; }

		public TargetWrapper CurrentTarget { get; private set; }

		public Data Setup(MechanicsContext context, TargetWrapper target)
		{
			Context = context;
			CurrentTarget = target;
			return this;
		}

		protected override void Reset()
		{
			Context = null;
			CurrentTarget = null;
		}
	}

	private static readonly int CountOfRanks = Enum.GetValues(typeof(AbilityRankType)).Cast<int>().Max() + 1;

	private static readonly int CountOfValues = Enum.GetValues(typeof(AbilitySharedValue)).Cast<int>().Max() + 1;

	private static readonly int CountOfProperties = Enum.GetValues(typeof(ContextPropertyName)).Cast<int>().Max() + 1;

	[JsonProperty]
	private readonly EntityRef<MechanicEntity> m_OwnerRef;

	[JsonProperty]
	private readonly EntityRef<MechanicEntity> m_CasterRef;

	[JsonProperty]
	[NotNull]
	private readonly int[] m_Ranks = new int[CountOfRanks];

	[JsonProperty]
	[NotNull]
	private readonly int[] m_SharedValues = new int[CountOfValues];

	[JsonProperty]
	[NotNull]
	public readonly BlueprintScriptableObject AssociatedBlueprint;

	[JsonProperty]
	[CanBeNull]
	private TargetWrapper m_MainTarget;

	[JsonProperty]
	[CanBeNull]
	private BlueprintAbility m_SourceAbility;

	[JsonProperty]
	private int m_ShadowFactorPercents;

	[JsonProperty]
	[CanBeNull]
	private List<ShadowDisbelieveData> m_ShadowDisbelieveData;

	[NotNull]
	[GameStateInclude]
	private readonly int[] m_Properties = new int[CountOfValues];

	private List<ContextRankConfig> m_RankSources;

	private List<ContextCalculateSharedValue> m_ValueSources;

	public readonly Dictionary<ContextActionDealDamage, int?> AoEDamage = new Dictionary<ContextActionDealDamage, int?>();

	[JsonProperty]
	[CanBeNull]
	public MechanicsContext ParentContext { get; private set; }

	[JsonProperty]
	public SpellDescriptor SpellDescriptor { get; private set; }

	[JsonProperty]
	public SpellSchool SpellSchool { get; private set; }

	[JsonProperty]
	public int SpellLevel { get; protected set; }

	[JsonProperty(IsReference = false)]
	public Vector3 Direction { get; protected set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[GameStateIgnore]
	private Dictionary<ContextPropertyName, int> PropertiesConverter
	{
		get
		{
			Dictionary<ContextPropertyName, int> dictionary = null;
			int num = Math.Min(m_Properties.Length, CountOfProperties);
			for (int i = 0; i < num; i++)
			{
				if (m_Properties[i] > 0)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<ContextPropertyName, int>();
					}
					dictionary.Add((ContextPropertyName)i, m_Properties[i]);
				}
			}
			return dictionary;
		}
		set
		{
			if (value == null)
			{
				return;
			}
			foreach (var (contextPropertyName2, num2) in value)
			{
				m_Properties[(int)contextPropertyName2] = num2;
			}
		}
	}

	public bool DisableFx { get; set; }

	[CanBeNull]
	public MechanicEntity MaybeCaster
	{
		get
		{
			MechanicEntity mechanicEntity = m_CasterRef.Entity;
			if (mechanicEntity == null)
			{
				if (!(MaybeOwner?.UniqueId == m_CasterRef.Id))
				{
					return null;
				}
				mechanicEntity = MaybeOwner;
			}
			return mechanicEntity;
		}
	}

	[CanBeNull]
	public MechanicEntity MaybeOwner => m_OwnerRef.Entity;

	[NotNull]
	public virtual TargetWrapper MainTarget
	{
		get
		{
			TargetWrapper targetWrapper = m_MainTarget;
			if ((object)targetWrapper == null)
			{
				MechanicEntity entity = m_OwnerRef.Entity;
				if (entity == null)
				{
					return new TargetWrapper(Vector3.zero);
				}
				targetWrapper = entity;
			}
			return targetWrapper;
		}
	}

	[CanBeNull]
	public BlueprintAbility SourceAbility
	{
		get
		{
			object obj = m_SourceAbility;
			if (obj == null)
			{
				obj = AssociatedBlueprint as BlueprintAbility;
				if (obj == null)
				{
					MechanicsContext parentContext = ParentContext;
					if (parentContext == null)
					{
						return null;
					}
					obj = parentContext.SourceAbility;
				}
			}
			return (BlueprintAbility)obj;
		}
	}

	public string Name => SelectUIData(UIDataType.Name)?.Name ?? "";

	public string Description => SelectUIData(UIDataType.Description)?.Description ?? "";

	public Sprite Icon => SelectUIData(UIDataType.Icon)?.Icon;

	public string NameForAcronym => SelectUIData(UIDataType.NameForAcronym)?.NameForAcronym ?? "";

	[NotNull]
	public MechanicsContext Root => ParentContext?.Root ?? this;

	[CanBeNull]
	public AbilityExecutionContext SourceAbilityContext => Root as AbilityExecutionContext;

	[CanBeNull]
	public RulePerformSavingThrow SavingThrow => ContextData<SavingThrowData>.Current?.Rule;

	public int ShadowFactorPercents
	{
		get
		{
			int valueOrDefault = (MaybeCaster?.GetOptional<UnitPartShadowSummon>()?.Context.ShadowFactorPercents).GetValueOrDefault();
			if (valueOrDefault == 0)
			{
				return m_ShadowFactorPercents;
			}
			if (m_ShadowFactorPercents == 0)
			{
				return valueOrDefault;
			}
			return Math.Min(valueOrDefault, m_ShadowFactorPercents);
		}
		set
		{
			m_ShadowFactorPercents = value;
		}
	}

	public float ShadowFactor => (float)ShadowFactorPercents / 100f;

	public bool IsShadow => ShadowFactorPercents > 0;

	public int this[AbilitySharedValue valueType]
	{
		get
		{
			return m_SharedValues[(int)valueType];
		}
		set
		{
			m_SharedValues[(int)valueType] = value;
		}
	}

	public int this[ContextPropertyName propertyName]
	{
		get
		{
			return m_Properties[(int)propertyName];
		}
		set
		{
			m_Properties[(int)propertyName] = value;
		}
	}

	public MechanicsContext(MechanicEntity caster, MechanicEntity owner, BlueprintScriptableObject blueprint, MechanicsContext parent = null, TargetWrapper mainTarget = null)
	{
		if (!blueprint)
		{
			throw new ArgumentNullException("blueprint");
		}
		if (caster == null && owner == null)
		{
			throw new Exception("caster and owner both null");
		}
		m_CasterRef = caster ?? owner;
		m_OwnerRef = owner ?? caster;
		AssociatedBlueprint = blueprint;
		ParentContext = parent;
		m_MainTarget = mainTarget;
		Direction = ParentContext?.Direction ?? Vector3.zero;
		if (parent != null)
		{
			for (int j = 0; j < m_SharedValues.Length && j < parent.m_SharedValues.Length; j++)
			{
				m_SharedValues[j] = parent.m_SharedValues[j];
			}
			for (int k = 0; k < m_Properties.Length && k < parent.m_Properties.Length; k++)
			{
				m_Properties[k] = parent.m_Properties[k];
			}
		}
		SpellDescriptor = (ParentContext?.SpellDescriptor ?? SpellDescriptor.None) | (SpellDescriptor)(AssociatedBlueprint.GetComponent<SpellDescriptorComponent>()?.Descriptor ?? ((SpellDescriptorWrapper)SpellDescriptor.None));
		SpellSchool = SpellSchool.None;
		SpellLevel = ParentContext?.SpellLevel ?? 0;
		m_ShadowFactorPercents = parent?.m_ShadowFactorPercents ?? 0;
		if (parent?.m_ShadowDisbelieveData != null)
		{
			m_ShadowDisbelieveData = parent.m_ShadowDisbelieveData.Select((ShadowDisbelieveData i) => i.Clone()).ToList();
		}
	}

	[JsonConstructor]
	protected MechanicsContext(JsonConstructorMark _)
	{
	}

	[CanBeNull]
	public IUIDataProvider SelectUIData(UIDataType type)
	{
		IUIDataProvider iUIDataProvider = AssociatedBlueprint as IUIDataProvider;
		switch (type)
		{
		case UIDataType.Name:
			if (!string.IsNullOrEmpty(iUIDataProvider?.Name))
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.Description:
			if (!string.IsNullOrEmpty(iUIDataProvider?.Description))
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.Icon:
			if (iUIDataProvider?.Icon != null)
			{
				return iUIDataProvider;
			}
			break;
		case UIDataType.NameForAcronym:
			if (!string.IsNullOrEmpty(iUIDataProvider?.NameForAcronym))
			{
				return iUIDataProvider;
			}
			break;
		}
		return ParentContext?.SelectUIData(type);
	}

	public void RecalculateSharedValues()
	{
		if (m_ValueSources == null)
		{
			m_ValueSources = AssociatedBlueprint.GetComponents<ContextCalculateSharedValue>().ToList();
		}
		foreach (ContextCalculateSharedValue valueSource in m_ValueSources)
		{
			this[valueSource.ValueType] = valueSource.Calculate(this);
		}
	}

	public void Recalculate()
	{
		if (MaybeCaster == null)
		{
			return;
		}
		RecalculateSharedValues();
		BlueprintComponent[] componentsArray = AssociatedBlueprint.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is PropertyCalculatorComponent { SaveToContext: not PropertyCalculatorComponent.SaveToContextType.No } propertyCalculatorComponent)
			{
				MechanicEntity mechanicEntity = ((propertyCalculatorComponent.SaveToContext == PropertyCalculatorComponent.SaveToContextType.ForCaster) ? MaybeCaster : MainTarget.Entity);
				if (mechanicEntity == null)
				{
					PFLog.Default.ErrorWithReport("Can't calculate property: unit is null ({0}, {1}, {2})", propertyCalculatorComponent.SaveToContext, propertyCalculatorComponent.Name, AssociatedBlueprint);
				}
				else
				{
					this[propertyCalculatorComponent.Name] = propertyCalculatorComponent.GetValue(new PropertyContext(mechanicEntity, null, null, this));
				}
			}
		}
	}

	public virtual MechanicsContext CloneFor([NotNull] BlueprintScriptableObject blueprint, [CanBeNull] MechanicEntity owner = null, [CanBeNull] MechanicEntity caster = null, [CanBeNull] TargetWrapper target = null)
	{
		return new MechanicsContext(caster ?? MaybeCaster, owner ?? m_OwnerRef.Entity, blueprint, this, target);
	}

	public void UnlinkParent()
	{
		ParentContext = null;
	}

	public virtual T TriggerRule<T>(T rule) where T : RulebookEvent
	{
		RuleReason left = rule.Reason;
		RuleReason right = default(RuleReason);
		if (left == right)
		{
			rule.Reason = this;
		}
		return Rulebook.Trigger(rule);
	}

	public void AddSpellDescriptor(SpellDescriptor spellDescriptor)
	{
		SpellDescriptor |= spellDescriptor;
	}

	public void RemoveSpellDescriptor(SpellDescriptor spellDescriptor)
	{
		SpellDescriptor &= ~spellDescriptor;
	}

	public Data GetDataScope(MechanicEntity target)
	{
		return GetDataScope(target.ToITargetWrapper());
	}

	public Data GetDataScope(ITargetWrapper target = null)
	{
		return ContextData<Data>.Request().Setup(this, ((TargetWrapper)target) ?? MainTarget);
	}

	public bool TryAffectByShadow(MechanicEntity target, bool checkChance)
	{
		if (!IsShadow)
		{
			return false;
		}
		ShadowDisbelieveData shadowDisbelieveData = m_ShadowDisbelieveData?.FirstItem((ShadowDisbelieveData i) => i.Unit == target);
		if (shadowDisbelieveData == null)
		{
			shadowDisbelieveData = new ShadowDisbelieveData
			{
				Unit = target
			};
			UnitPartShadowSummon unitPartShadowSummon = MaybeCaster?.GetOptional<UnitPartShadowSummon>();
			if (unitPartShadowSummon != null)
			{
				shadowDisbelieveData.IsDisbelieved = !unitPartShadowSummon.Context.TryAffectByShadow(target, checkChance: false);
			}
			else
			{
				RulePerformSavingThrow evt = new RulePerformSavingThrow(target, SavingThrowType.Will, 0)
				{
					Reason = this
				};
				shadowDisbelieveData.IsDisbelieved = Rulebook.Trigger(evt).IsPassed;
			}
			if (m_ShadowDisbelieveData == null)
			{
				m_ShadowDisbelieveData = new List<ShadowDisbelieveData>();
			}
			m_ShadowDisbelieveData.Add(shadowDisbelieveData);
		}
		if (shadowDisbelieveData.IsDisbelieved && checkChance && !shadowDisbelieveData.IsChanceChecked)
		{
			RuleRollChance evt2 = new RuleRollChance(target, ShadowFactorPercents, null, RollChanceType.AffectedByShadow)
			{
				Reason = this
			};
			shadowDisbelieveData.IsAffectedByChance = Rulebook.Trigger(evt2).Success;
			shadowDisbelieveData.IsChanceChecked = true;
		}
		if (shadowDisbelieveData.IsDisbelieved)
		{
			if (checkChance)
			{
				return shadowDisbelieveData.IsAffectedByChance;
			}
			return false;
		}
		return true;
	}

	public void SetSourceAbility([CanBeNull] BlueprintAbility sourceAbility)
	{
		m_SourceAbility = sourceAbility;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = m_OwnerRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		EntityRef<MechanicEntity> obj2 = m_CasterRef;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj2);
		result.Append(ref val2);
		result.Append(m_Ranks);
		result.Append(m_SharedValues);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(AssociatedBlueprint);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<MechanicsContext>.GetHash128(ParentContext);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<TargetWrapper>.GetHash128(m_MainTarget);
		result.Append(ref val5);
		Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_SourceAbility);
		result.Append(ref val6);
		result.Append(ref m_ShadowFactorPercents);
		List<ShadowDisbelieveData> shadowDisbelieveData = m_ShadowDisbelieveData;
		if (shadowDisbelieveData != null)
		{
			for (int i = 0; i < shadowDisbelieveData.Count; i++)
			{
				Hash128 val7 = ClassHasher<ShadowDisbelieveData>.GetHash128(shadowDisbelieveData[i]);
				result.Append(ref val7);
			}
		}
		SpellDescriptor val8 = SpellDescriptor;
		result.Append(ref val8);
		SpellSchool val9 = SpellSchool;
		result.Append(ref val9);
		int val10 = SpellLevel;
		result.Append(ref val10);
		Vector3 val11 = Direction;
		result.Append(ref val11);
		result.Append(m_Properties);
		return result;
	}
}
