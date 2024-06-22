using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
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

	private readonly RankEntrySelectionVM m_Owner;

	private CalculatedPrerequisite Prerequisite => m_SelectionState.Value?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, m_Caster);

	public TooltipTemplateRankEntryFeature(UIFeature uiFeature, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, RankEntrySelectionVM owner, BaseUnitEntity caster = null)
		: base(uiFeature)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Caster = caster;
		m_Owner = owner;
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
		if (Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature.RankEntryUtils.IsCommonSelectionItem(m_SelectionItem))
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.CommonFeatureDesc, TooltipTitleType.H4));
		}
		list.AddRange(base.GetHeader(type));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.AddRange(base.GetBody(type));
		if (Prerequisite != null)
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			AddPrerequisiteGroup(UIUtility.GetPrerequisiteEntries(Prerequisite), list);
		}
		return list;
	}

	private static void AddPrerequisiteGroup(List<PrerequisiteEntryVM> allPrerequisites, List<ITooltipBrick> result, bool isOr = false)
	{
		List<PrerequisiteEntryVM> list = allPrerequisites.Where((PrerequisiteEntryVM p) => !p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list2 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list3 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.IsGroup).ToList();
		if (isOr && list.Any())
		{
			AddOr(result);
		}
		result.Add(new TooltipBrickPrerequisite(list));
		if (list2.Any())
		{
			if (isOr)
			{
				AddOr(result);
			}
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
			result.Add(new TooltipBrickPrerequisite(list2));
		}
		if (isOr && list3.Any())
		{
			AddOr(result);
		}
		for (int i = 0; i < list3.Count; i++)
		{
			if (isOr && i > 0)
			{
				AddOr(result);
			}
			PrerequisiteEntryVM prerequisiteEntryVM = list3[i];
			AddPrerequisiteGroup(prerequisiteEntryVM.Prerequisites, result, prerequisiteEntryVM.IsOrComposition);
		}
	}

	private static void AddOr(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickItemHeader($"<size={UIConfig.Instance.SubTextPercentSize}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type != 0 && Game.Instance.IsControllerMouse && Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature.RankEntryUtils.HasPrerequisiteFooter(Prerequisite, m_Owner))
		{
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.PrerequisitesFooter, TooltipTitleType.H6);
		}
	}
}
