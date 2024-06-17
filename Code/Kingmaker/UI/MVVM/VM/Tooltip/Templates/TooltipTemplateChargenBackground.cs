using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.Utility;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateChargenBackground : TooltipBaseTemplate
{
	private readonly BlueprintFeature m_Feature;

	private readonly bool m_IsInfoWindow;

	private int ColumnCount
	{
		get
		{
			if (!m_IsInfoWindow)
			{
				return 1;
			}
			return 2;
		}
	}

	public TooltipTemplateChargenBackground(BlueprintFeature feature, bool isInfoWindow = true)
	{
		m_Feature = feature;
		m_IsInfoWindow = isInfoWindow;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Feature.Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		AddFeatures(list);
		AddStatBonuses(list, StatTypeHelper.Attributes, UIStrings.Instance.CharGen.BackgroundStatsBonuses);
		AddStatBonuses(list, StatTypeHelper.Skills, UIStrings.Instance.CharGen.BackgroundSkillsBonuses);
		IEnumerable<AddFeaturesToLevelUp> components = m_Feature.GetComponents<AddFeaturesToLevelUp>();
		if (components.Any())
		{
			list.Add(new TooltipBrickText(UIStrings.Instance.CharGen.BackgroundUnlockedFeaturesForLevelUp, TooltipTextType.Centered));
			AddLevelUpStats(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Attribute), UIStrings.Instance.CharGen.BackgroundStatsForLevelUp);
			AddLevelUpStats(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Skill), UIStrings.Instance.CharGen.BackgroundSkillsForLevelUp);
			AddLevelUpFeatures(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.ActiveAbility), UIStrings.Instance.CharacterSheet.Abilities);
			AddLevelUpFeatures(list, components.Where((AddFeaturesToLevelUp i) => i.Group == FeatureGroup.Talent), UIStrings.Instance.CharGen.BackgroundTalentsForLevelUp);
		}
		return list;
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Feature.Description))
		{
			string text = UIUtilityTexts.UpdateDescriptionWithUIProperties(m_Feature.Description, null);
			bricks.Add(new TooltipBrickText(text));
		}
	}

	private void AddFeatures(List<ITooltipBrick> bricks)
	{
		IEnumerable<BlueprintUnitFact> source = m_Feature.GetComponents<AddFacts>().SelectMany((AddFacts i) => i.Facts);
		if (source.Any())
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharGen.BackgroundFeatures, TooltipTitleType.H3));
			TooltipBricksGroupLayoutParams tooltipBricksGroupLayoutParams = new TooltipBricksGroupLayoutParams
			{
				LayoutType = TooltipBricksGroupLayoutType.Grid,
				ColumnCount = ColumnCount,
				Padding = new RectOffset(11, 11, 0, 0),
				PreferredElementHeight = 62f
			};
			if (m_IsInfoWindow)
			{
				tooltipBricksGroupLayoutParams.PreferredElementHeight = 72f;
				tooltipBricksGroupLayoutParams.CellSize = new Vector2(300f, 72f);
			}
			bricks.Add(new TooltipBricksGroupStart(hasBackground: false, tooltipBricksGroupLayoutParams));
			bricks.AddRange(source.Select((BlueprintUnitFact fact) => new TooltipBrickFeature(fact, isHeader: false, FeatureTypes.Expanded)));
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddStatBonuses(List<ITooltipBrick> bricks, StatType[] types, string title)
	{
		IEnumerable<AddContextStatBonus> enumerable = from s in m_Feature.GetComponents<AddContextStatBonus>()
			where types.Contains(s.Stat)
			select s;
		if (!enumerable.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H3));
		TooltipBricksGroupLayoutParams tooltipBricksGroupLayoutParams = new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Grid,
			ColumnCount = ColumnCount,
			Padding = new RectOffset(21, 21, 0, 0)
		};
		if (m_IsInfoWindow)
		{
			tooltipBricksGroupLayoutParams.Padding = new RectOffset(0, 0, 0, 0);
			tooltipBricksGroupLayoutParams.Spacing = new Vector2(10f, 4f);
			tooltipBricksGroupLayoutParams.CellSize = new Vector2(300f, 30f);
		}
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, tooltipBricksGroupLayoutParams));
		foreach (AddContextStatBonus item in enumerable)
		{
			string statShortLabel = GetStatShortLabel(item.Stat);
			string text = LocalizedTexts.Instance.Stats.GetText(item.Stat);
			string valueWithSign = UIConstsExtensions.GetValueWithSign(item.Value.Value);
			TooltipBrickIconStatValueType tooltipBrickIconStatValueType = ((item.Value.Value > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			Color value = ((tooltipBrickIconStatValueType == TooltipBrickIconStatValueType.Positive) ? UIConfig.Instance.StatPositiveColor : UIConfig.Instance.StatNegativeColor);
			TooltipTemplateGlossary tooltipTemplateGlossary = new TooltipTemplateGlossary(item.Stat.ToString());
			bricks.Add(new TooltipBrickIconStatValue(text, valueWithSign, null, type: tooltipBrickIconStatValueType, tooltip: tooltipTemplateGlossary, iconSize: 40f, iconText: statShortLabel, iconColor: value, icon: UIConfig.Instance.UIIcons.StatBackground));
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private string GetStatShortLabel(StatType statType)
	{
		return UIUtilityTexts.GetStatShortName(UIUtilityUnit.GetSourceStatType(Game.Instance.Player.MainCharacterEntity.Stats.Container.GetStat(statType)) ?? statType);
	}

	private void AddLevelUpStats(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> levelUpFeatures, string title)
	{
		if (!levelUpFeatures.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H3));
		TooltipBricksGroupLayoutParams tooltipBricksGroupLayoutParams = new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Grid,
			ColumnCount = ColumnCount,
			Padding = new RectOffset(21, 21, 0, 0)
		};
		if (m_IsInfoWindow)
		{
			tooltipBricksGroupLayoutParams.Padding = new RectOffset(0, 0, 0, 0);
			tooltipBricksGroupLayoutParams.Spacing = new Vector2(10f, 4f);
			tooltipBricksGroupLayoutParams.CellSize = new Vector2(300f, 30f);
		}
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, tooltipBricksGroupLayoutParams));
		foreach (BlueprintFeature item in levelUpFeatures.SelectMany((AddFeaturesToLevelUp i) => i.Features))
		{
			if (item is BlueprintStatAdvancement blueprintStatAdvancement)
			{
				string statShortLabel = GetStatShortLabel(blueprintStatAdvancement.Stat);
				string text = LocalizedTexts.Instance.Stats.GetText(blueprintStatAdvancement.Stat);
				string valueWithSign = UIConstsExtensions.GetValueWithSign(blueprintStatAdvancement.ValuePerRank);
				TooltipTemplateGlossary tooltipTemplateGlossary = new TooltipTemplateGlossary(blueprintStatAdvancement.Stat.ToString());
				bricks.Add(new TooltipBrickIconStatValue(text, valueWithSign, null, tooltip: tooltipTemplateGlossary, iconSize: 40f, iconText: statShortLabel, iconColor: UIConfig.Instance.StatPositiveColor, icon: UIConfig.Instance.UIIcons.StatBackground, type: TooltipBrickIconStatValueType.Positive));
			}
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddLevelUpFeatures(List<ITooltipBrick> bricks, IEnumerable<AddFeaturesToLevelUp> levelUpFeatures, string title)
	{
		if (!levelUpFeatures.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H3));
		TooltipBricksGroupLayoutParams tooltipBricksGroupLayoutParams = new TooltipBricksGroupLayoutParams
		{
			LayoutType = TooltipBricksGroupLayoutType.Grid,
			ColumnCount = ColumnCount,
			Padding = new RectOffset(10, 10, 0, 0),
			PreferredElementHeight = 62f
		};
		if (m_IsInfoWindow)
		{
			tooltipBricksGroupLayoutParams.PreferredElementHeight = 72f;
			tooltipBricksGroupLayoutParams.CellSize = new Vector2(300f, 72f);
		}
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, tooltipBricksGroupLayoutParams));
		foreach (AddFeaturesToLevelUp levelUpFeature in levelUpFeatures)
		{
			foreach (BlueprintFeature feature in levelUpFeature.Features)
			{
				bricks.Add(new TooltipBrickFeature(feature, isHeader: false, available: true, showIcon: true, FeatureTypes.Expanded));
			}
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}
}
