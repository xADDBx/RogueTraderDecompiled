using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Levelup.Obsolete;

[Obsolete]
public class LevelUpState
{
	public enum CharBuildMode
	{
		LevelUp,
		CharGen,
		Respec,
		Mythic,
		SetName
	}

	public readonly CharBuildMode Mode;

	public readonly int NextCharacterLevel;

	public readonly int NextMythicLevel;

	[NotNull]
	public readonly List<FeatureSelectionState> Selections = new List<FeatureSelectionState>();

	[NotNull]
	public readonly List<SpellSelectionData> SpellSelections = new List<SpellSelectionData>();

	[NotNull]
	public BaseUnitEntity TargetUnit { get; }

	[NotNull]
	public BaseUnitEntity PreviewUnit { get; }

	public bool IsPregen { get; private set; }

	public int NextClassLevel { get; set; }

	[CanBeNull]
	public BlueprintCharacterClass SelectedClass { get; set; }

	public int AttributePoints { get; set; }

	public int TotalSkillPoints { get; set; }

	public int SpentSkillPoints { get; set; }

	public bool CanSelectAlignment { get; set; }

	public bool CanSelectRace { get; set; }

	public bool CanSelectRaceStat { get; set; }

	[CanBeNull]
	public StatType? SelectedRaceStat { get; set; }

	public bool CanSelectName { get; set; }

	public bool CanSelectPortrait { get; set; }

	public bool CanSelectGender { get; set; }

	public bool CanSelectVoice { get; set; }

	public bool IsClassAutoselect { get; set; }

	public bool IsEmployee
	{
		get
		{
			if (PreviewUnit.IsCustomCompanion())
			{
				return true;
			}
			return false;
		}
	}

	public bool IsLoreCompanion => PreviewUnit.IsStoryCompanion();

	public int SkillPointsRemaining => TotalSkillPoints - SpentSkillPoints;

	public bool IsFirstCharacterLevel => NextCharacterLevel == 1;

	public bool IsMythicClassSelected
	{
		get
		{
			if (SelectedClass != null)
			{
				return SelectedClass.IsMythic;
			}
			return false;
		}
	}

	public LevelUpState([NotNull] BaseUnitEntity targetUnit, CharBuildMode mode, bool isPregen)
		: this(targetUnit, targetUnit, mode, isPregen)
	{
	}

	public LevelUpState([NotNull] BaseUnitEntity targetUnit, [NotNull] BaseUnitEntity previewUnit, CharBuildMode mode, bool isPregen)
	{
		TargetUnit = targetUnit ?? throw new ArgumentNullException("targetUnit");
		PreviewUnit = previewUnit ?? throw new ArgumentNullException("previewUnit");
		IsPregen = isPregen;
		if (LevelUpController.NeedToSetName(previewUnit))
		{
			CanSelectName = true;
			Mode = CharBuildMode.SetName;
			return;
		}
		NextCharacterLevel = previewUnit.Progression.CharacterLevel + 1;
		NextMythicLevel = previewUnit.Progression.MythicLevel + 1;
		if (IsFirstCharacterLevel)
		{
			Mode = ((mode == CharBuildMode.LevelUp) ? CharBuildMode.CharGen : mode);
			bool flag = Mode != CharBuildMode.Respec || !IsLoreCompanion;
			CanSelectPortrait = Mode == CharBuildMode.CharGen || (Mode == CharBuildMode.Respec && !IsLoreCompanion);
			CanSelectRace = flag;
			CanSelectGender = Mode == CharBuildMode.CharGen || (Mode == CharBuildMode.Respec && !IsLoreCompanion);
			CanSelectAlignment = flag;
			CanSelectName = flag;
			CanSelectVoice = flag;
			return;
		}
		switch (mode)
		{
		case CharBuildMode.Mythic:
			Mode = CharBuildMode.Mythic;
			return;
		case CharBuildMode.Respec:
			return;
		}
		if (mode != CharBuildMode.Mythic)
		{
			Mode = CharBuildMode.LevelUp;
		}
	}

	[NotNull]
	public FeatureSelectionState AddSelection([CanBeNull] FeatureSelectionState parent, FeatureSource source, [NotNull] IFeatureSelection selection, int selectionLevel)
	{
		int index = Selections.Count((FeatureSelectionState s) => s.Selection == selection);
		FeatureSelectionState featureSelectionState = new FeatureSelectionState(parent, source, selection, index, selectionLevel);
		Selections.Add(featureSelectionState);
		return featureSelectionState;
	}

	[CanBeNull]
	public FeatureSelectionState FindSelection([NotNull] IFeatureSelection selection, bool allowSelected = false)
	{
		return Selections.FirstOrDefault((FeatureSelectionState s) => s.Selection == selection && (!s.Selected || allowSelected));
	}

	public bool HasSelection(FeatureSelectionState selectionState)
	{
		return Selections.Contains(selectionState);
	}

	public int RemainingSelections()
	{
		return Selections.Count((FeatureSelectionState s) => !s.Selected && s.CanSelectAnything(this));
	}

	[CanBeNull]
	public SpellSelectionData GetSpellSelection([NotNull] BlueprintSpellbook spellbook, [NotNull] BlueprintSpellList spellList)
	{
		return SpellSelections.FirstOrDefault((SpellSelectionData s) => s.Spellbook == spellbook && s.SpellList == spellList);
	}

	[NotNull]
	public SpellSelectionData DemandSpellSelection([NotNull] BlueprintSpellbook spellbook, [CanBeNull] BlueprintSpellList spellList)
	{
		SpellSelectionData spellSelectionData = SpellSelections.FirstOrDefault((SpellSelectionData s) => s.Spellbook == spellbook && s.SpellList == spellList);
		if (spellSelectionData == null)
		{
			spellSelectionData = new SpellSelectionData(spellbook, spellList);
			SpellSelections.Add(spellSelectionData);
		}
		return spellSelectionData;
	}

	public bool IsAlignmentRestricted(Alignment alignment)
	{
		return false;
	}

	public bool IsComplete()
	{
		if (CanSelectAlignment)
		{
			return false;
		}
		if (CanSelectRace)
		{
			return false;
		}
		if (CanSelectRaceStat)
		{
			return false;
		}
		if (CanSelectName)
		{
			return false;
		}
		if (CanSelectPortrait)
		{
			return false;
		}
		if (CanSelectGender)
		{
			return false;
		}
		if (CanSelectVoice)
		{
			return false;
		}
		if (AttributePoints > 0)
		{
			return false;
		}
		if (SelectedClass == null)
		{
			return false;
		}
		if (!IsSkillPointsComplete())
		{
			return false;
		}
		if (Selections.Any((FeatureSelectionState s) => !s.Selected && s.CanSelectAnything(this)))
		{
			return false;
		}
		if (SpellSelections.Any((SpellSelectionData data) => data.CanSelectAnything(PreviewUnit)))
		{
			return false;
		}
		return true;
	}

	public bool IsSkillPointsComplete()
	{
		return true;
	}

	public void OnApplyAction()
	{
		foreach (SpellSelectionData spellSelection in SpellSelections)
		{
			spellSelection.UpdateMaxLevelSpells(PreviewUnit);
		}
	}

	public void SetPregenMode(bool isPregen)
	{
		IsPregen = isPregen;
	}
}
