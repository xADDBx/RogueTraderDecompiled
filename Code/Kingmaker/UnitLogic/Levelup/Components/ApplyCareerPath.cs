using System;
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
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

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
				flag = !selectionStateFeature.CanSelectAny || SelectFeature(selectionStateFeature);
			}
			if (!flag)
			{
				PFLog.LevelUp.ErrorWithReport($"ApplyCareerPath: can't find suitable option for selection ${selection.Blueprint} " + $"in path ${selection.Path}[${selection.PathRank}] " + $"({base.Owner})");
			}
		}
	}

	private bool SelectFeature(SelectionStateFeature selection)
	{
		ReferenceArrayProxy<BlueprintFeature>? referenceArrayProxy = Selections.FirstItem((SelectionEntry i) => i.Group == selection.Blueprint.Group)?.Items;
		if (referenceArrayProxy.HasValue)
		{
			foreach (BlueprintFeature feature in referenceArrayProxy.Value)
			{
				FeatureSelectionItem selectionItem = selection.Items.FirstItem((FeatureSelectionItem i) => i.Feature == feature && selection.CanSelect(i));
				if (selectionItem.Feature != null)
				{
					selection.Select(selectionItem);
					return true;
				}
			}
		}
		FeatureSelectionItem selectionItem2 = selection.Items.FirstItem(selection.CanSelect);
		if (selectionItem2.Feature == null)
		{
			return false;
		}
		selection.Select(selectionItem2);
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
