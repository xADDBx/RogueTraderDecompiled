using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using MemoryPack;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features;

[Serializable]
[TypeId("cb208b98ceacca84baee15dba53b1979")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintFeature : BlueprintFeatureBase, IFeatureSelectionItem, IUIDataProvider, IBlueprintFactWithRanks
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintFeature>
	{
	}

	public enum FeatureType
	{
		Offense,
		Defense,
		Support,
		Universal,
		Archetype,
		Origin,
		Warp
	}

	private NameModifier[] m_NameModifiersCache;

	private DescriptionModifier[] m_DescriptionModifiersCache;

	[NotNull]
	public static List<BlueprintFeature> EmptyList = new List<BlueprintFeature>();

	public int Ranks = 1;

	public PrerequisitesList Prerequisites = new PrerequisitesList();

	public List<FeatureType> FeatureTypes = new List<FeatureType>();

	public TalentIconInfo TalentIconInfo = new TalentIconInfo();

	[SerializeField]
	private LocalizedString m_Acronym;

	public bool ShowInDialogue;

	public bool IsStarshipFeature;

	public string ForceSetNameForItemTooltip
	{
		get
		{
			AddForceSetName component = this.GetComponent<AddForceSetName>();
			if (component != null)
			{
				return component.ForceName;
			}
			return string.Empty;
		}
	}

	FeatureParam IFeatureSelectionItem.Param => null;

	BlueprintFeature IFeatureSelectionItem.Feature => this;

	public override string Name
	{
		get
		{
			if (m_NameModifiersCache == null)
			{
				m_NameModifiersCache = this.GetComponents<NameModifier>().ToArray();
			}
			string text = base.Name;
			NameModifier[] nameModifiersCache = m_NameModifiersCache;
			for (int i = 0; i < nameModifiersCache.Length; i++)
			{
				text = nameModifiersCache[i].Modify(text);
			}
			return text;
		}
	}

	public override string Description
	{
		get
		{
			if (m_DescriptionModifiersCache == null)
			{
				m_DescriptionModifiersCache = this.GetComponents<DescriptionModifier>().ToArray();
			}
			string text = base.Description;
			DescriptionModifier[] descriptionModifiersCache = m_DescriptionModifiersCache;
			for (int i = 0; i < descriptionModifiersCache.Length; i++)
			{
				text = descriptionModifiersCache[i].Modify(text);
			}
			return text;
		}
	}

	public string Acronym => m_Acronym?.Text;

	public bool MeetsPrerequisites([NotNull] LevelUpState state, bool fromProgression, [CanBeNull] FeatureSelectionState selectionState)
	{
		return MeetsPrerequisites(state.PreviewUnit, fromProgression, selectionState, state);
	}

	public virtual bool MeetsPrerequisites([NotNull] BaseUnitEntity unit, bool fromProgression, [CanBeNull] FeatureSelectionState selectionState, [CanBeNull] LevelUpState state)
	{
		if (IgnorePrerequisites.Ignore && !fromProgression)
		{
			return true;
		}
		if (unit.Progression.Features.GetRank(this) >= Ranks && !(this is IFeatureSelection))
		{
			return false;
		}
		bool? flag = null;
		bool? flag2 = null;
		for (int i = 0; i < base.ComponentsArray.Length; i++)
		{
			Prerequisite_Obsolete prerequisite_Obsolete = base.ComponentsArray[i] as Prerequisite_Obsolete;
			if ((bool)prerequisite_Obsolete && (!fromProgression || prerequisite_Obsolete.CheckInProgression))
			{
				bool flag3 = prerequisite_Obsolete.Check(selectionState, unit, state);
				if (prerequisite_Obsolete.Group == Prerequisite_Obsolete.GroupType.All)
				{
					flag = ((!flag.HasValue) ? flag3 : (flag.Value && flag3));
				}
				else
				{
					flag2 = ((!flag2.HasValue) ? flag3 : (flag2.Value || flag3));
				}
			}
		}
		if (flag ?? true)
		{
			return flag2 ?? true;
		}
		return false;
	}

	public void RestrictPrerequisites([CanBeNull] FeatureSelectionState selectionState, [NotNull] BaseUnitEntity unit, [NotNull] LevelUpState state)
	{
		if (IgnorePrerequisites.Ignore)
		{
			return;
		}
		for (int i = 0; i < base.ComponentsArray.Length; i++)
		{
			Prerequisite_Obsolete prerequisite_Obsolete = base.ComponentsArray[i] as Prerequisite_Obsolete;
			if ((bool)prerequisite_Obsolete)
			{
				prerequisite_Obsolete.Restrict(selectionState, unit, state);
			}
		}
	}

	public override MechanicEntityFact CreateFact(MechanicsContext parentContext, MechanicEntity owner, BuffDuration duration)
	{
		return new Feature(this, (BaseUnitEntity)owner, parentContext);
	}
}
