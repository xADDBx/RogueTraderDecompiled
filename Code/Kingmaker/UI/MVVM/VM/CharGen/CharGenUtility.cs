using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public static class CharGenUtility
{
	public static T GetSelectionByType<T>(BlueprintPath path) where T : BlueprintSelection
	{
		return path.RankEntries.SelectMany((BlueprintPath.RankEntry i) => i.Selections).OfType<T>().FirstOrDefault();
	}

	public static List<StatType> GetSelectedCareerRecommendedStats<T>(BaseUnitEntity unit) where T : BlueprintStatAdvancement
	{
		if (unit?.Progression == null)
		{
			return new List<StatType>();
		}
		return GetCareerRecommendedStats<T>(unit.Progression.AllCareerPaths.FirstOrDefault(((BlueprintCareerPath Blueprint, int Rank) c) => c.Blueprint.Tier == CareerPathTier.One).Blueprint.GetComponent<CareerPathUIMetaData>());
	}

	public static List<StatType> GetCareerRecommendedStats<T>(CareerPathUIMetaData metaData) where T : BlueprintStatAdvancement
	{
		return ((metaData != null) ? (from f in metaData.RecommendedFeatures.OfType<T>()
			select f.Stat).ToList() : null) ?? new List<StatType>();
	}

	public static IEnumerable<BlueprintSelectionFeature> GetFeatureSelectionsByGroup(BlueprintPath path, FeatureGroup group, BaseUnitEntity unit = null)
	{
		for (int i = 1; i <= path.RankEntries.Length; i++)
		{
			BlueprintPath.RankEntry rankEntry = path.GetRankEntry(i);
			if (rankEntry != null)
			{
				IEnumerable<BlueprintSelectionFeature> enumerable = from s in rankEntry.Selections.OfType<BlueprintSelectionFeature>()
					where (unit == null || s.MeetPrerequisites(unit)) && s.Group == @group
					select s;
				if (enumerable.Any())
				{
					return enumerable;
				}
			}
		}
		return Enumerable.Empty<BlueprintSelectionFeature>();
	}

	public static IEnumerable<KingmakerEquipmentEntity> GetUnitEquipmentEntities(BaseUnitEntity unit)
	{
		return from i in unit.Progression.Features.Enumerable.Select((Feature f) => f.Blueprint.GetComponent<AddKingmakerEquipmentEntity>()).NotNull()
			select i.EquipmentEntity;
	}

	public static IEnumerable<EquipmentEntityLink> GetClothes(IEnumerable<KingmakerEquipmentEntity> equipmentEntities, Gender gender, Race race)
	{
		return equipmentEntities.Where((KingmakerEquipmentEntity e) => e != null).SelectMany((KingmakerEquipmentEntity e) => e.GetLinks(gender, race));
	}

	public static CharacterColorsProfile GetClothesColorsProfile(List<EquipmentEntityLink> eeClothes, bool secondary = false)
	{
		foreach (EquipmentEntityLink eeClothe in eeClothes)
		{
			EquipmentEntity equipmentEntity = eeClothe.Load();
			if (equipmentEntity == null)
			{
				continue;
			}
			if (!secondary)
			{
				if (equipmentEntity.PrimaryColorsProfile == null)
				{
					PFLog.TechArt.Error("No primary colors profile in " + equipmentEntity.name);
					continue;
				}
			}
			else if (equipmentEntity.SecondaryColorsProfile == null)
			{
				PFLog.TechArt.Error("No secondary colors profile in " + equipmentEntity.name);
				continue;
			}
			CharacterColorsProfile characterColorsProfile = (secondary ? equipmentEntity.SecondaryColorsProfile : equipmentEntity.PrimaryColorsProfile);
			if (characterColorsProfile != null)
			{
				return characterColorsProfile;
			}
		}
		return null;
	}

	public static CharacterColorsProfile GetClothesColorsProfile(List<EquipmentEntityLink> clothes, out RampColorPreset colorPreset, bool secondary = false)
	{
		for (int i = 0; i < clothes.Count; i++)
		{
			EquipmentEntity equipmentEntity = clothes[i].Load();
			if (!(equipmentEntity == null))
			{
				CharacterColorsProfile characterColorsProfile = (secondary ? equipmentEntity.SecondaryColorsProfile : equipmentEntity.PrimaryColorsProfile);
				if (characterColorsProfile != null)
				{
					colorPreset = equipmentEntity.ColorPresets;
					return characterColorsProfile;
				}
			}
		}
		colorPreset = null;
		return null;
	}

	public static BlueprintOriginPath GetUnitOriginPath(BaseUnitEntity unit)
	{
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewGameCustomChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewGameCustomChargenPath;
		}
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewGamePregenChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewGamePregenChargenPath;
		}
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewCompanionCustomChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewCompanionCustomChargenPath;
		}
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewCompanionPregenChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewCompanionPregenChargenPath;
		}
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewCompanionNavigatorCustomChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewCompanionNavigatorCustomChargenPath;
		}
		if (unit.Progression.GetPathRank(BlueprintCharGenRoot.Instance.NewCompanionNavigatorPregenChargenPath) > 0)
		{
			return BlueprintCharGenRoot.Instance.NewCompanionNavigatorPregenChargenPath;
		}
		return null;
	}
}
