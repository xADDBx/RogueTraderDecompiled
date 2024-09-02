using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCareer : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	private readonly CareerPathUIMetaData m_CareerPathUIMetaData;

	private readonly bool m_IsScreenView;

	public TooltipTemplateCareer(CareerPathVM careerPath, bool isScreenView = false)
	{
		m_CareerPath = careerPath;
		m_CareerPathUIMetaData = careerPath.CareerPath.GetComponent<CareerPathUIMetaData>();
		m_IsScreenView = isScreenView;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_CareerPath.Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info)
		{
			AddPrerequisites(list);
		}
		list.Add(new TooltipBrickText(m_CareerPath.Description, TooltipTextType.Paragraph));
		AddKeystoneAbilities(list);
		AddUltimateAbilities(list);
		if (type == TooltipTemplateType.Info)
		{
			AddStatsAndSkills(list);
		}
		return list;
	}

	private void AddStatsAndSkills(List<ITooltipBrick> bricks)
	{
		if (Enumerable.Any(m_CareerPath.AdvancementAttributes))
		{
			List<StatType> careerRecommendedStats = CharGenUtility.GetCareerRecommendedStats<BlueprintAttributeAdvancement>(m_CareerPathUIMetaData);
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharacterSheet.Stats.Text, TooltipTitleType.H5));
			bricks.Add(new TooltipBricksGroupStart(hasBackground: true, GetStatLayoutParams()));
			foreach (BlueprintAttributeAdvancement advancementAttribute in m_CareerPath.AdvancementAttributes)
			{
				string text = LocalizedTexts.Instance.Stats.GetText(advancementAttribute.Stat);
				string statShortName = UIUtilityTexts.GetStatShortName(advancementAttribute.Stat);
				bool isRecommended = careerRecommendedStats.Contains(advancementAttribute.Stat);
				TooltipTemplateStat tooltip = new TooltipTemplateStat(new StatTooltipData(m_CareerPath.Unit.Stats.Container.GetStat(advancementAttribute.Stat)));
				bricks.Add(new TooltipBrickAttribute(text, statShortName, tooltip, StripeType.Stat, isRecommended));
			}
			bricks.Add(new TooltipBricksGroupEnd());
		}
		if (!Enumerable.Any(m_CareerPath.AdvancementSkills))
		{
			return;
		}
		List<StatType> careerRecommendedStats2 = CharGenUtility.GetCareerRecommendedStats<BlueprintSkillAdvancement>(m_CareerPathUIMetaData);
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CharacterSheet.Skills.Text, TooltipTitleType.H5));
		bricks.Add(new TooltipBricksGroupStart(hasBackground: true, GetStatLayoutParams()));
		foreach (BlueprintSkillAdvancement advancementSkill in m_CareerPath.AdvancementSkills)
		{
			string text2 = LocalizedTexts.Instance.Stats.GetText(advancementSkill.Stat);
			ModifiableValue stat = m_CareerPath.Unit.Stats.Container.GetStat(advancementSkill.Stat);
			string statShortName2 = UIUtilityTexts.GetStatShortName(UIUtilityUnit.GetSourceStatType(stat) ?? advancementSkill.Stat);
			bool isRecommended2 = careerRecommendedStats2.Contains(advancementSkill.Stat);
			TooltipTemplateStat tooltip2 = new TooltipTemplateStat(new StatTooltipData(stat));
			bricks.Add(new TooltipBrickAttribute(text2, statShortName2, tooltip2, StripeType.Skill, isRecommended2));
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddFeaturesGroup(List<ITooltipBrick> bricks, IEnumerable<BlueprintUnitFact> features, string header)
	{
		if (features.Any())
		{
			bricks.Add(new TooltipBrickTitle(header, TooltipTitleType.H5));
			bricks.Add(new TooltipBricksGroupStart(hasBackground: true, GetLayoutParams()));
			bricks.AddRange(from feature in features.NotNull()
				select new TooltipBrickFeature(feature));
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddKeystoneAbilities(List<ITooltipBrick> bricks)
	{
		if (m_CareerPathUIMetaData != null)
		{
			List<BlueprintUnitFact> list = new List<BlueprintUnitFact>();
			list.AddRange(m_CareerPathUIMetaData.KeystoneAbilities);
			list.AddRange(m_CareerPathUIMetaData.KeystoneFeatures);
			AddFeaturesGroup(bricks, list, UIStrings.Instance.CharacterSheet.KeystoneFeaturesHeader);
		}
	}

	private void AddUltimateAbilities(List<ITooltipBrick> bricks)
	{
		if (m_CareerPathUIMetaData != null)
		{
			IEnumerable<BlueprintAbility> features = m_CareerPathUIMetaData.UltimateFeatures.Select((BlueprintFeature ultimateFeature) => ultimateFeature.GetComponent<AddFacts>()).NotNull().SelectMany((AddFacts i) => i.Facts)
				.Cast<BlueprintAbility>();
			AddFeaturesGroup(bricks, features, UIStrings.Instance.CharacterSheet.UltimateAbilitiesHeader);
		}
	}

	private void AddPrerequisites(List<ITooltipBrick> bricks)
	{
		if (m_CareerPath.Prerequisite != null && !m_CareerPath.IsUnlocked)
		{
			bricks.Add(new TooltipBrickPrerequisite(UIUtility.GetPrerequisiteEntries(m_CareerPath.Prerequisite)));
		}
	}

	private TooltipBricksGroupLayoutParams GetLayoutParams()
	{
		float value = (Game.Instance.IsControllerGamepad ? 76f : 62f);
		if (m_IsScreenView)
		{
			return new TooltipBricksGroupLayoutParams
			{
				LayoutType = TooltipBricksGroupLayoutType.Grid,
				Padding = new RectOffset(6, 6, -6, 0),
				Spacing = new Vector2(0f, -4f),
				ColumnCount = 1,
				PreferredElementHeight = value
			};
		}
		return null;
	}

	private TooltipBricksGroupLayoutParams GetStatLayoutParams()
	{
		float value = (Game.Instance.IsControllerGamepad ? 40f : 35f);
		if (m_IsScreenView)
		{
			return new TooltipBricksGroupLayoutParams
			{
				LayoutType = TooltipBricksGroupLayoutType.Grid,
				Padding = new RectOffset(0, 0, 0, 0),
				Spacing = new Vector2(0f, 3f),
				ColumnCount = 1,
				PreferredElementHeight = value
			};
		}
		return null;
	}
}
