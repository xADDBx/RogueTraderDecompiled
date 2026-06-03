using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterName;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.UnitLogic.Levelup.Selections.Ship;
using Kingmaker.UnitLogic.Levelup.Selections.Voice;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Levelup.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("e85c079c48fe4978beddcb7b61475362")]
public class ApplyCareerPath : UnitFactComponentDelegate, IHashable
{
	[Serializable]
	public class SelectionEntry
	{
		public FeatureGroup Group;

		[SerializeField]
		[ValidateNotEmpty]
		[ValidateNoNullEntries]
		private BlueprintFeature.Reference[] m_Items = new BlueprintFeature.Reference[0];

		public ReferenceArrayProxy<BlueprintFeature> Items
		{
			get
			{
				BlueprintReference<BlueprintFeature>[] items = m_Items;
				return items;
			}
		}

		public void AddItem(BlueprintFeature feature)
		{
			Array.Resize(ref m_Items, m_Items.Length + 1);
			m_Items[^1] = feature.ToReference<BlueprintFeature.Reference>();
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPath.Reference m_CareerPath;

	public int Ranks;

	[SerializeField]
	private bool m_UsePriorityBasedSelection = true;

	public SelectionEntry[] Selections = new SelectionEntry[0];

	public BlueprintPath CareerPath
	{
		get
		{
			return m_CareerPath;
		}
		set
		{
			m_CareerPath = value.ToReference<BlueprintPath.Reference>();
		}
	}

	protected override void OnActivate()
	{
		if (CareerPath is BlueprintCareerPath { IsAvailable: false })
		{
			return;
		}
		bool num = CareerPath is BlueprintOriginPath;
		int num2 = (num ? int.MaxValue : (base.Owner.OriginalBlueprint.GetComponent<CharacterLevelLimit>()?.LevelLimit ?? int.MaxValue));
		int rank = base.Owner.Progression.GetRank(CareerPath);
		int characterLevel = base.Owner.Progression.CharacterLevel;
		int num3 = ((!num) ? ((characterLevel >= num2) ? num2 : Math.Clamp(characterLevel + Ranks - rank, characterLevel, num2)) : 0);
		if (!num && num3 <= characterLevel)
		{
			return;
		}
		base.Owner.Progression.AdvanceExperienceToLevel(num3, log: false);
		if (base.Owner.IsPreviewUnit)
		{
			return;
		}
		Dictionary<FeatureGroup, List<BlueprintFeature>> value;
		using (CollectionPool<Dictionary<FeatureGroup, List<BlueprintFeature>>, KeyValuePair<FeatureGroup, List<BlueprintFeature>>>.Get(out value))
		{
			SelectionEntry[] selections = Selections;
			foreach (SelectionEntry selectionEntry in selections)
			{
				if (value.TryGetValue(selectionEntry.Group, out var value2))
				{
					value2.AddRange(selectionEntry.Items);
					continue;
				}
				List<BlueprintFeature> list = TempList.Get<BlueprintFeature>();
				list.AddRange(selectionEntry.Items);
				value[selectionEntry.Group] = list;
			}
			foreach (SelectionState selection in new LevelUpManager(base.Owner, CareerPath, autoCommit: true, num3).Selections)
			{
				bool flag;
				if (!(selection is SelectionStateFeature selectionStateFeature))
				{
					if (!(selection is SelectionStateDoll) && !(selection is SelectionStatePortrait) && !(selection is SelectionStateCharacterName) && !(selection is SelectionStateShip) && !(selection is SelectionStateVoice) && !(selection is SelectionStateGender))
					{
						throw new ArgumentOutOfRangeException("selection");
					}
					flag = true;
				}
				else
				{
					flag = !selectionStateFeature.CanSelectAny || SelectFeature(selectionStateFeature, value) || SelectDefaultFeature(selectionStateFeature);
				}
				if (!flag)
				{
					PFLog.LevelUp.ErrorWithReport($"ApplyCareerPath: can't find suitable option for selection ${selection.Blueprint} " + $"in path ${selection.Path}[${selection.PathRank}] " + $"({base.Owner})");
				}
			}
			if (m_UsePriorityBasedSelection)
			{
				return;
			}
			foreach (KeyValuePair<FeatureGroup, List<BlueprintFeature>> item in value)
			{
				if (item.Value != null && item.Value.Count > 0)
				{
					PFLog.LevelUp.Error($"ApplyCareerPath: Failed to apply features for the group {item.Key} in career {CareerPath?.name} for unit {base.Owner.CharacterName}: " + string.Join(",\n", item.Value.Select((BlueprintFeature f) => f.name)));
				}
			}
		}
	}

	private bool SelectFeature(SelectionStateFeature selection, Dictionary<FeatureGroup, List<BlueprintFeature>> presetSelections)
	{
		if (presetSelections.TryGetValue(selection.Blueprint.Group, out var value) && value != null)
		{
			for (int j = 0; j < value.Count; j++)
			{
				BlueprintFeature candidateFeature = value[j];
				FeatureSelectionItem selectionItem = selection.Items.FirstItem((FeatureSelectionItem i) => i.Feature == candidateFeature && selection.CanSelect(i));
				if (selectionItem.Feature != null)
				{
					selection.Select(selectionItem);
					if (!m_UsePriorityBasedSelection)
					{
						value.RemoveAt(j);
					}
					return true;
				}
			}
		}
		return false;
	}

	private bool SelectDefaultFeature(SelectionStateFeature selection)
	{
		FeatureSelectionItem selectionItem = selection.Items.FirstItem(selection.CanSelect);
		if (selectionItem.Feature == null)
		{
			return false;
		}
		selection.Select(selectionItem);
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
