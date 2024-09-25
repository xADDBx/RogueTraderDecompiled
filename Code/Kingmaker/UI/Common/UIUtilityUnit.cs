using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UnitLogic;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common.UIConfigComponents;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.UI.Common;

public static class UIUtilityUnit
{
	public enum UnitFractionViewMode
	{
		Companion,
		Enemy,
		Friend
	}

	public enum PortraitCombatSize
	{
		Icon,
		Small,
		Middle
	}

	public enum SkillRecommendationEnum
	{
		Bad,
		Normal,
		Best
	}

	private static EnumUnitSubtypeIcons SubtypeIcons => UIConfig.Instance.Portraits.UnitSubtypeIcons;

	private static EnumUnitSubtypeIcons SubtypePortrait => UIConfig.Instance.Portraits.UnitSubtypePortrait;

	public static bool FeatureIsActive(BlueprintFeatureBase feature)
	{
		AddFacts component = feature.GetComponent<AddFacts>();
		if (component == null)
		{
			return false;
		}
		return component.Facts.Any((BlueprintUnitFact f) => f is BlueprintAbility || f is BlueprintActivatableAbility);
	}

	public static List<int> GetSpellNumberTable(Spellbook spellbook)
	{
		List<int> list = new List<int>();
		for (int i = 0; i <= spellbook.LastSpellbookLevel; i++)
		{
			int num = 0;
			num = ((!spellbook.Blueprint.Spontaneous) ? (spellbook.CalcSlotsLimit(i, SpellSlotType.Common) + spellbook.CalcSlotsLimit(i, SpellSlotType.Favorite)) : spellbook.GetSpellsPerDay(i));
			list.Add(num);
		}
		return list;
	}

	internal static List<int> GetSpellNumberTable(BlueprintSpellbook spellbook, int classLevel)
	{
		List<int> list = new List<int>();
		for (int i = 0; i <= spellbook.MaxSpellLevel; i++)
		{
			int item = 0;
			int? count = spellbook.SpellsPerDay.GetCount(classLevel, i);
			if (count.HasValue)
			{
				item = count.Value;
			}
			list.Add(item);
		}
		return list;
	}

	public static List<int> GetSpellNumberBaseTable(Spellbook spellbook)
	{
		List<int> list = new List<int>();
		for (int i = 0; i <= spellbook.LastSpellbookLevel; i++)
		{
			int item = 0;
			int? count = spellbook.Blueprint.SpellsPerDay.GetCount(spellbook.CasterLevel, i);
			if (count.HasValue)
			{
				item = count.Value;
			}
			list.Add(item);
		}
		return list;
	}

	public static IEnumerable<Ability> CollectAbilities(BaseUnitEntity unit)
	{
		return unit.Abilities.Visible.Where((Ability a) => !a.Blueprint.IsCantrip && a.SourceItem == null);
	}

	public static IEnumerable<TBlueprint> GetBlueprintUnitFactFromFact<TBlueprint>(BlueprintMechanicEntityFact blueprintMechanicEntityFact) where TBlueprint : BlueprintUnitFact
	{
		ReferenceArrayProxy<BlueprintUnitFact>? referenceArrayProxy = blueprintMechanicEntityFact.GetComponent<AddFacts>()?.Facts;
		IEnumerable<TBlueprint> enumerable = (referenceArrayProxy.HasValue ? referenceArrayProxy.GetValueOrDefault().OfType<TBlueprint>() : null);
		if (blueprintMechanicEntityFact.GetComponent<AddFact>()?.Fact is TBlueprint element)
		{
			(enumerable ?? Array.Empty<TBlueprint>()).Append(element);
		}
		return enumerable;
	}

	public static IEnumerable<FeatureSelectorSlotVM> CollectAbilitiesVMs(BaseUnitEntity unit)
	{
		return from a in CollectAbilities(unit)
			select new FeatureSelectorSlotVM(a, unit);
	}

	public static IEnumerable<ActivatableAbility> CollectActivatableAbilities(BaseUnitEntity unit)
	{
		return unit.ActivatableAbilities.Visible;
	}

	public static IEnumerable<UIFeature> CollectAbilityFeatures(BaseUnitEntity unit)
	{
		return from f in CollectFeatures(unit)
			select new UIFeature(f, f.Param);
	}

	public static IEnumerable<Feature> CollectFeatures(BaseUnitEntity unit)
	{
		return from f in unit.Progression.Features.Visible
			where !f.Blueprint.HideInCharacterSheetAndLevelUp
			where f.Blueprint.Name != string.Empty
			where !(f.Blueprint is IFeatureSelection)
			where !(f.Blueprint is BlueprintProgression)
			select f;
	}

	public static IEnumerable<UIFeature> CollectFeats(BaseUnitEntity unit)
	{
		return from f in unit.Progression.Features.Visible
			where !f.Blueprint.HideInCharacterSheetAndLevelUp
			where f.Blueprint.Name != string.Empty
			where !(f.Blueprint is BlueprintFeatureSelection_Obsolete)
			where !(f.Blueprint is BlueprintProgression)
			select new UIFeature(f, f.Param);
	}

	public static IEnumerable<UIFeature> CollectTraits(BaseUnitEntity unit)
	{
		return from f in unit.Progression.Features.Visible
			where !f.Blueprint.HideInCharacterSheetAndLevelUp
			where f.Blueprint.Name != string.Empty
			where !(f.Blueprint is BlueprintFeatureSelection_Obsolete)
			where !(f.Blueprint is BlueprintProgression)
			select new UIFeature(f, f.Param);
	}

	public static IEnumerable<AbilityData> CollectSpells(BaseUnitEntity unit)
	{
		return unit.Spellbooks.SelectMany((Spellbook sb) => sb.GetAllKnownSpells());
	}

	public static StatType? GetSourceStatType(ModifiableValue stat)
	{
		if (stat == null)
		{
			return null;
		}
		if (stat is ModifiableValueSkill modifiableValueSkill)
		{
			return modifiableValueSkill.BaseStat.Type;
		}
		return null;
	}

	public static int GetSurfaceEnemyDifficulty([CanBeNull] BaseUnitEntity unit)
	{
		if (unit == null)
		{
			return 0;
		}
		return (int)(unit.Blueprint.DifficultyType + 1);
	}

	public static bool UsedSubtypeIcon(MechanicEntity mechanicEntityEntity)
	{
		return mechanicEntityEntity?.GetUnitUISettingsOptional()?.Portrait.SmallPortrait == null;
	}

	public static Sprite GetSurfaceCombatStandardPortrait(MechanicEntity mechanicEntityEntity, PortraitCombatSize size)
	{
		PortraitData portraitData = mechanicEntityEntity?.GetUnitUISettingsOptional()?.Portrait;
		UnitSubtype val = (mechanicEntityEntity as UnitEntity)?.Blueprint.Subtype ?? UnitSubtype.Default;
		return size switch
		{
			PortraitCombatSize.Icon => portraitData?.SmallPortrait ?? SubtypeIcons.GetSprite(val), 
			PortraitCombatSize.Small => portraitData?.SmallPortrait ?? SubtypePortrait.GetSprite(val), 
			PortraitCombatSize.Middle => portraitData?.HalfLengthPortrait ?? SubtypePortrait.GetSprite(val), 
			_ => portraitData?.SmallPortrait ?? SubtypePortrait.GetSprite(val), 
		};
	}

	public static SkillRecommendationEnum GetSkillGraduation(StatType stat, BaseUnitEntity unit, List<BaseUnitEntity> from)
	{
		if (unit == null || from == null)
		{
			return SkillRecommendationEnum.Bad;
		}
		ModifiableValue statOptional = unit.GetStatOptional(stat);
		return (from u in @from
			select u.GetStatOptional(stat)?.ModifiedValue ?? 0 into x
			orderby -x
			select x).Distinct().ToList().IndexOf(statOptional) switch
		{
			0 => SkillRecommendationEnum.Best, 
			1 => SkillRecommendationEnum.Normal, 
			_ => SkillRecommendationEnum.Bad, 
		};
	}

	public static bool IsCastingAbility([CanBeNull] this BaseUnitEntity unit)
	{
		return unit?.Commands?.Current is UnitUseAbility;
	}

	public static bool InPartyAndControllable(this MechanicEntity unit)
	{
		PartFaction factionOptional = unit.GetFactionOptional();
		if ((object)factionOptional != null && factionOptional.IsDirectlyControllable)
		{
			return unit.CanBeControlled();
		}
		return false;
	}
}
