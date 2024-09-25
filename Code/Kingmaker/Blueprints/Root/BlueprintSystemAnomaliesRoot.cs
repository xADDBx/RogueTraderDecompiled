using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("631060c9b85a4ffc93fbda557eca628c")]
public class BlueprintSystemAnomaliesRoot : BlueprintScriptableObject
{
	[Serializable]
	public class StatName
	{
		public StatType Skill;

		public LocalizedString Name;
	}

	[Serializable]
	public class SkillToFacts
	{
		public StatType Skill;

		public FactWeights Facts;
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintSystemAnomaliesRoot>
	{
	}

	public AnomalyGroupWeights AnomalyGroupSpawns;

	public StatName[] SkillCheckTypePool;

	public FactWeights FactsPool;

	[SerializeField]
	private SkillToFacts[] m_SkillToFacts;

	public int MinAnomaliesInSystem = 1;

	public int MaxAnomaliesInSystem = 2;

	[Range(0f, 100f)]
	public float NoAnomaliesPercentage = 10f;

	public int SkillCheckCountOnScan = 3;

	public List<SkillToFacts> SkillToFactsList => m_SkillToFacts.ToList();
}
