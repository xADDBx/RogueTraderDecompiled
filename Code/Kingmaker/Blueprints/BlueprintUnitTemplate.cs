using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.QA;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("087baeb698a722645a7ec6660a397916")]
public class BlueprintUnitTemplate : BlueprintScriptableObject
{
	[Serializable]
	public class StatAdjustment
	{
		public int Adjustment;

		public StatType Stat;
	}

	[SerializeField]
	[FormerlySerializedAs("RemoveFacts")]
	private BlueprintUnitFactReference[] m_RemoveFacts;

	[SerializeField]
	[FormerlySerializedAs("AddFacts")]
	private BlueprintUnitFactReference[] m_AddFacts;

	public StatAdjustment[] StatAdjustments;

	public ReferenceArrayProxy<BlueprintUnitFact> RemoveFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] removeFacts = m_RemoveFacts;
			return removeFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> AddFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] addFacts = m_AddFacts;
			return addFacts;
		}
	}

	public void ApplyTemplate(BaseUnitEntity unit)
	{
		foreach (BlueprintUnitFact item in AddFacts.EmptyIfNull().NotNull())
		{
			unit.AddFact(item)?.AddSource(this);
		}
		foreach (BlueprintUnitFact item2 in RemoveFacts.EmptyIfNull().NotNull())
		{
			unit.AddFact(item2)?.AddSource(this);
		}
		StatAdjustment[] array = StatAdjustments.EmptyIfNull();
		foreach (StatAdjustment statAdjustment in array)
		{
			ModifiableValue statOptional = unit.Stats.GetStatOptional(statAdjustment.Stat);
			if (statOptional != null)
			{
				if (!(statOptional is ModifiableValueAttributeStat modifiableValueAttributeStat) || (modifiableValueAttributeStat.Enabled && modifiableValueAttributeStat.BaseValue > 2))
				{
					statOptional.BaseValue += statAdjustment.Adjustment;
				}
			}
			else
			{
				PFLog.Default.ErrorWithReport(this, $"Cannot change stat {statAdjustment.Stat}: stat not found");
			}
		}
	}
}
