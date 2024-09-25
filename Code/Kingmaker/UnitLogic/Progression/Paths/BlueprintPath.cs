using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[TypeId("3eb928a0df1049099c82edc91d03d8da")]
[MemoryPackable(GenerateType.NoGenerate)]
public abstract class BlueprintPath : BlueprintFeature
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintPath>
	{
	}

	[Serializable]
	public class RankEntry
	{
		[SerializeField]
		[ItemNotNull]
		private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

		[SerializeField]
		[ItemNotNull]
		private BlueprintSelection.Reference[] m_Selections = new BlueprintSelection.Reference[0];

		public ReferenceArrayProxy<BlueprintFeature> Features
		{
			get
			{
				BlueprintReference<BlueprintFeature>[] features = m_Features;
				return features;
			}
		}

		public ReferenceArrayProxy<BlueprintSelection> Selections
		{
			get
			{
				BlueprintReference<BlueprintSelection>[] selections = m_Selections;
				return selections;
			}
		}
	}

	[InfoBox("Ranks must be equal to RankEntries.Length")]
	[ArrayElementNamePrefix("Rank", true)]
	public RankEntry[] RankEntries = new RankEntry[0];

	[CanBeNull]
	public RankEntry GetRankEntry(int level)
	{
		return RankEntries.Get(level - 1);
	}
}
