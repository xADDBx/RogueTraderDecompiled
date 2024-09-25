using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateFeature : TooltipBaseTemplate
{
	public readonly BlueprintFeatureBase BlueprintFeatureBase;

	private readonly Feature m_Feature;

	private readonly MechanicEntity m_Caster;

	private readonly bool m_WithVariants;

	private string m_Description;

	private (BlueprintAbility HeroicAct, BlueprintAbility DesperateMeasure) m_Abilities;

	public override void Prepare(TooltipTemplateType type)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				if (BlueprintFeatureBase == null)
				{
					throw new ArgumentNullException("BlueprintFeature is NUll");
				}
				if (m_WithVariants)
				{
					m_Abilities = UIUtility.GetUltimateAbilities(BlueprintFeatureBase);
				}
				m_Description = TooltipTemplateUtils.GetFullDescription(BlueprintFeatureBase);
				m_Description = UIUtilityTexts.UpdateDescriptionWithUIProperties(m_Description, m_Feature?.ConcreteOwner ?? m_Caster);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {BlueprintFeatureBase?.name}: {arg}");
		}
	}

	public TooltipTemplateFeature([NotNull] Feature feature, bool withVariants = false)
		: this(feature?.Blueprint, withVariants, feature?.Owner)
	{
		m_Feature = feature;
	}

	public TooltipTemplateFeature(BlueprintFeatureBase feature, bool withVariants = false, MechanicEntity caster = null)
	{
		BlueprintFeatureBase = feature;
		m_WithVariants = withVariants;
		m_Caster = caster;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (!m_WithVariants)
		{
			string acronym = ((BlueprintFeatureBase.Icon != null) ? "" : UIUtility.GetAbilityAcronym(BlueprintFeatureBase.Name));
			Sprite icon = ((BlueprintFeatureBase.Icon != null) ? BlueprintFeatureBase.Icon : UIUtility.GetIconByText(BlueprintFeatureBase.NameForAcronym));
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = BlueprintFeatureBase.Name,
				TextParams = new TextFieldParams
				{
					FontStyles = FontStyles.Bold
				}
			};
			TalentIconInfo iconsInfo = (BlueprintFeatureBase as BlueprintFeature)?.TalentIconInfo;
			yield return new TooltipBrickIconPattern(icon, null, titleValues, null, null, null, IconPatternMode.SkillMode, acronym, iconsInfo);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_Abilities.DesperateMeasure != null || m_Abilities.HeroicAct != null)
		{
			if (m_Abilities.HeroicAct != null)
			{
				yield return new TooltipBrickText(UIStrings.Instance.Tooltips.HeroicActAbility, TooltipTextType.Bold);
				yield return new TooltipBrickFeature(m_Abilities.HeroicAct);
			}
			if (m_Abilities.DesperateMeasure != null)
			{
				yield return new TooltipBrickText(UIStrings.Instance.Tooltips.DesperateMeasureAbility, TooltipTextType.Bold);
				yield return new TooltipBrickFeature(m_Abilities.DesperateMeasure);
			}
			yield break;
		}
		yield return new TooltipBrickText(m_Description, TooltipTextType.Paragraph);
		ITooltipBrick sourceBrick = null;
		if ((bool)m_Feature?.SourceAbilityBlueprint)
		{
			sourceBrick = new TooltipBrickFeature(m_Feature.SourceAbilityBlueprint);
		}
		if (m_Feature?.SourceFact != null && !string.IsNullOrEmpty(m_Feature?.SourceFact.Name))
		{
			sourceBrick = new TooltipBrickFeature(m_Feature.SourceFact);
		}
		if (m_Feature?.SourceItem != null)
		{
			ItemEntity itemEntity = (ItemEntity)m_Feature.SourceItem;
			sourceBrick = new TooltipBrickIconAndName(itemEntity.Icon, itemEntity.Name);
		}
		if ((bool)m_Feature?.SourceClass)
		{
			sourceBrick = new TooltipBrickIconAndName(m_Feature.SourceClass.Icon, m_Feature.SourceClass.Name);
		}
		Feature feature = m_Feature;
		if (!string.IsNullOrEmpty((feature == null) ? null : SimpleBlueprintExtendAsObject.Or(feature.SourceProgression, null)?.Name))
		{
			sourceBrick = new TooltipBrickFeature(m_Feature.SourceProgression);
		}
		if ((bool)m_Feature?.SourceRace)
		{
			sourceBrick = new TooltipBrickFeature(m_Feature.SourceRace);
		}
		if (sourceBrick != null)
		{
			yield return new TooltipBrickSeparator();
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2);
			yield return sourceBrick;
		}
	}
}
