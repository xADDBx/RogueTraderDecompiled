using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("973c613b8443cf14495c283e293d35f9")]
public class BlueprintAbilityResource : BlueprintScriptableObject, IUIDataProvider
{
	[Serializable]
	private struct Amount
	{
		[UsedImplicitly]
		public int BaseValue;

		[UsedImplicitly]
		public bool IncreasedByLevel;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevel")]
		[SerializeField]
		[FormerlySerializedAs("Class")]
		private BlueprintCharacterClassReference[] m_Class;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevel")]
		[SerializeField]
		[FormerlySerializedAs("Archetypes")]
		private BlueprintArchetypeReference[] m_Archetypes;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevel")]
		public int LevelIncrease;

		[UsedImplicitly]
		public bool IncreasedByLevelStartPlusDivStep;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int StartingLevel;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int StartingIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int LevelStep;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int PerStepIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public int MinClassLevelIncrease;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		[SerializeField]
		[FormerlySerializedAs("ClassDiv")]
		private BlueprintCharacterClassReference[] m_ClassDiv;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevel")]
		[SerializeField]
		[FormerlySerializedAs("ArchetypesDiv")]
		private BlueprintArchetypeReference[] m_ArchetypesDiv;

		[UsedImplicitly]
		[ShowIf("IncreasedByLevelStartPlusDivStep")]
		public float OtherClassesModifier;

		[UsedImplicitly]
		public bool IncreasedByStat;

		[UsedImplicitly]
		[ShowIf("IncreasedByStat")]
		public StatType ResourceBonusStat;

		public ReferenceArrayProxy<BlueprintCharacterClass> Class
		{
			get
			{
				BlueprintReference<BlueprintCharacterClass>[] @class = m_Class;
				return new ReferenceArrayProxy<BlueprintCharacterClass>(@class);
			}
		}

		public ReferenceArrayProxy<BlueprintArchetype> Archetypes
		{
			get
			{
				BlueprintReference<BlueprintArchetype>[] archetypes = m_Archetypes;
				return new ReferenceArrayProxy<BlueprintArchetype>(archetypes);
			}
		}

		public ReferenceArrayProxy<BlueprintCharacterClass> ClassDiv
		{
			get
			{
				BlueprintReference<BlueprintCharacterClass>[] classDiv = m_ClassDiv;
				return new ReferenceArrayProxy<BlueprintCharacterClass>(classDiv);
			}
		}

		public ReferenceArrayProxy<BlueprintArchetype> ArchetypesDiv
		{
			get
			{
				BlueprintReference<BlueprintArchetype>[] archetypesDiv = m_ArchetypesDiv;
				return new ReferenceArrayProxy<BlueprintArchetype>(archetypesDiv);
			}
		}
	}

	[NotNull]
	public LocalizedString LocalizedName;

	[NotNull]
	public LocalizedString LocalizedDescription;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private Amount m_MaxAmount;

	[SerializeField]
	private bool m_UseMax;

	[SerializeField]
	[ShowIf("m_UseMax")]
	private int m_Max = 10;

	[SerializeField]
	[InfoBox("Resource would be restored to at least this amount (Useful for MaxAmount dependent on stat modifier, that can be negative)")]
	private int m_Min;

	public string Name => LocalizedName;

	public string Description => LocalizedDescription;

	public Sprite Icon => m_Icon;

	public string NameForAcronym => name;

	public int GetMaxAmount(Entity entity)
	{
		int num = m_MaxAmount.BaseValue;
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return num;
		}
		foreach (BlueprintCharacterClass item in m_MaxAmount.Class)
		{
			if (!m_MaxAmount.IncreasedByLevel)
			{
				continue;
			}
			bool flag = true;
			foreach (BlueprintArchetype archetype in m_MaxAmount.Archetypes)
			{
				ClassData classData = baseUnitEntity.Progression.GetClassData(item);
				flag = classData == null || !item.Archetypes.Contains(archetype) || classData.Archetypes.Contains(archetype);
			}
			num += (flag ? (m_MaxAmount.LevelIncrease * baseUnitEntity.Progression.GetClassLevel(item)) : 0);
		}
		if (m_MaxAmount.IncreasedByStat)
		{
			if (baseUnitEntity.Stats.GetStat(m_MaxAmount.ResourceBonusStat) is ModifiableValueAttributeStat modifiableValueAttributeStat)
			{
				num += modifiableValueAttributeStat.Bonus;
			}
			else
			{
				PFLog.Default.Error("Can't use stat {0} in ability resource's count formula", m_MaxAmount.ResourceBonusStat);
			}
		}
		if (m_MaxAmount.IncreasedByLevelStartPlusDivStep)
		{
			int num2 = 0;
			foreach (BlueprintCharacterClass item2 in m_MaxAmount.ClassDiv)
			{
				bool flag2 = true;
				foreach (BlueprintArchetype item3 in m_MaxAmount.ArchetypesDiv)
				{
					ClassData classData2 = baseUnitEntity.Progression.GetClassData(item2);
					flag2 = classData2 == null || !item2.Archetypes.Contains(item3) || classData2.Archetypes.Contains(item3);
				}
				num2 += (flag2 ? baseUnitEntity.Progression.GetClassLevel(item2) : 0);
			}
			num2 += (int)((float)(baseUnitEntity.Progression.CharacterLevel - num2) * m_MaxAmount.OtherClassesModifier);
			if (m_MaxAmount.StartingLevel <= num2)
			{
				num += Math.Max(m_MaxAmount.StartingIncrease + m_MaxAmount.PerStepIncrease * (num2 - m_MaxAmount.StartingLevel) / m_MaxAmount.LevelStep, m_MaxAmount.MinClassLevelIncrease);
			}
		}
		int bonus = 0;
		EventBus.RaiseEvent((IBaseUnitEntity)baseUnitEntity, (Action<IResourceAmountBonusHandler>)delegate(IResourceAmountBonusHandler h)
		{
			h.CalculateMaxResourceAmount(this, ref bonus);
		}, isCheckRuntime: true);
		return Math.Max(m_Min, ApplyMinMax(num) + bonus);
	}

	private int ApplyMinMax(int result)
	{
		if (m_UseMax)
		{
			result = Math.Min(result, m_Max);
		}
		return result;
	}
}
