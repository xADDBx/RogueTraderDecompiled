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
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Components;
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

	private readonly bool m_IsCharGen;

	private static readonly TooltipBricksGroupLayoutParams AttributesTooltipLayout = new TooltipBricksGroupLayoutParams
	{
		LayoutType = TooltipBricksGroupLayoutType.Vertical,
		Padding = new RectOffset(21, 21, 0, 0)
	};

	private static readonly TooltipBricksGroupLayoutParams AttributesInfoLayout = new TooltipBricksGroupLayoutParams
	{
		LayoutType = TooltipBricksGroupLayoutType.Grid,
		ColumnCount = 1,
		Padding = new RectOffset(0, 0, 0, 0),
		Spacing = new Vector2(10f, 0f),
		CellSize = new Vector2(550f, 34f * SettingsRoot.Accessiability.FontSizeMultiplier)
	};

	private static readonly TooltipBricksGroupLayoutParams FeaturesTooltipLayout = new TooltipBricksGroupLayoutParams
	{
		LayoutType = TooltipBricksGroupLayoutType.Grid,
		ColumnCount = 1,
		Padding = new RectOffset(11, 11, 0, 0),
		PreferredElementHeight = 62f
	};

	private static readonly TooltipBricksGroupLayoutParams FeaturesInfoLayout = new TooltipBricksGroupLayoutParams
	{
		LayoutType = TooltipBricksGroupLayoutType.Grid,
		ColumnCount = 2,
		Padding = new RectOffset(10, 10, 0, 0),
		PreferredElementHeight = 72f,
		CellSize = new Vector2(300f, 72f)
	};

	public TooltipTemplateChargenBackground(BlueprintFeature feature, bool isInfoWindow = true, bool isCharGen = false)
	{
		m_Feature = feature;
		m_IsInfoWindow = isInfoWindow;
		m_IsCharGen = isCharGen;
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
		BlueprintComponentsEnumerator<AddFeaturesToLevelUp> components = m_Feature.GetComponents<AddFeaturesToLevelUp>();
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
		ReplaceDescriptionForCharGen component;
		string text = ((m_IsCharGen && m_Feature.TryGetComponent<ReplaceDescriptionForCharGen>(out component)) ? ((string)component.CharGenDescription) : m_Feature.Description);
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = UIUtilityTexts.UpdateDescriptionWithUIProperties(text, null);
			bricks.Add(new TooltipBrickText(text2));
		}
	}

	private void AddFeatures(List<ITooltipBrick> bricks)
	{
		IEnumerable<BlueprintUnitFact> source = m_Feature.GetComponents<AddFacts>().SelectMany((AddFacts i) => i.Facts);
		if (source.Any())
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharGen.BackgroundFeatures, TooltipTitleType.H3));
			TooltipBricksGroupLayoutParams layoutParams = (m_IsInfoWindow ? FeaturesInfoLayout : FeaturesTooltipLayout);
			bricks.Add(new TooltipBricksGroupStart(hasBackground: false, layoutParams));
			bricks.AddRange(source.Select((BlueprintUnitFact fact) => new TooltipBrickFeature(fact)));
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
		TooltipBricksGroupLayoutParams layoutParams = (m_IsInfoWindow ? AttributesInfoLayout : AttributesTooltipLayout);
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, layoutParams));
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
		TooltipBricksGroupLayoutParams layoutParams = (m_IsInfoWindow ? AttributesInfoLayout : AttributesTooltipLayout);
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, layoutParams));
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
		TooltipBricksGroupLayoutParams layoutParams = (m_IsInfoWindow ? FeaturesInfoLayout : FeaturesTooltipLayout);
		bricks.Add(new TooltipBricksGroupStart(hasBackground: false, layoutParams));
		foreach (AddFeaturesToLevelUp levelUpFeature in levelUpFeatures)
		{
			foreach (BlueprintFeature feature in levelUpFeature.Features)
			{
				bricks.Add(new TooltipBrickFeature(feature));
			}
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}
}
