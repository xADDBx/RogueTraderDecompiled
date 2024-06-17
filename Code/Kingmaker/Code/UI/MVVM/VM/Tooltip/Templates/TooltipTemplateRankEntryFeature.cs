using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRankEntryFeature : TooltipTemplateUIFeature
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly IReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	private readonly BaseUnitEntity m_Caster;

	public TooltipTemplateRankEntryFeature(UIFeature uiFeature, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, BaseUnitEntity caster = null)
		: base(uiFeature)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Caster = caster;
	}

	protected override void AddDescription(List<ITooltipBrick> bricks)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				string fullDescription = TooltipTemplateUtils.GetFullDescription(UIFeature.Feature);
				fullDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, m_Caster);
				bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, null)));
			}
		}
		catch (Exception arg)
		{
			bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(UIFeature.Description, null)));
			Debug.LogError($"Can't create TooltipTemplate for: {UIFeature.Feature.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (RankEntryUtils.IsCommonSelectionItem(m_SelectionItem))
		{
			list.Add(new TooltipBrickSpace(2f));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.CommonFeatureDesc, TooltipTitleType.H4));
			list.Add(new TooltipBrickSpace());
		}
		list.AddRange(base.GetHeader(type));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.AddRange(base.GetBody(type));
		CalculatedPrerequisite calculatedPrerequisite = m_SelectionState.Value?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, m_Caster);
		if (calculatedPrerequisite != null)
		{
			list.Add(new TooltipBrickSpace(4f));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			List<PrerequisiteEntryVM> prerequisiteEntries = UIUtility.GetPrerequisiteEntries(calculatedPrerequisite);
			List<PrerequisiteEntryVM> list2 = prerequisiteEntries.Where((PrerequisiteEntryVM p) => !p.Inverted).ToList();
			List<PrerequisiteEntryVM> list3 = prerequisiteEntries.Except(list2).ToList();
			list.Add(new TooltipBrickPrerequisite(list2));
			if (list3.Any())
			{
				list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
				list.Add(new TooltipBrickPrerequisite(list3));
			}
		}
		return list;
	}
}
