using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.Utility.StatefulRandom;
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

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit;

	private CalculatedPrerequisite Prerequisite => m_SelectionState.Value?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, m_Caster);

	public TooltipTemplateRankEntryFeature(UIFeature uiFeature, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, RankEntrySelectionVM owner, BaseUnitEntity caster, ReactiveProperty<BaseUnitEntity> previewUnit)
		: base(uiFeature)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Caster = caster;
		m_PreviewUnit = previewUnit;
		m_Owner = owner;
	}

	protected override void AddDescription(List<ITooltipBrick> bricks)
	{
		try
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
				{
					using (ContextData<UnitHelper.PreviewUnit>.Request())
					{
						using (GameLogContext.Scope)
						{
							GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_PreviewUnit.Value;
							EntityFact entityFact = null;
							if (m_PreviewUnit.Value.Facts.Get<UnitFact>(UIFeature.Feature) == null)
							{
								entityFact = m_PreviewUnit.Value.AddFact(UIFeature.Feature);
							}
							string fullDescription = TooltipTemplateUtils.GetFullDescription(UIFeature.Feature);
							fullDescription = UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, m_PreviewUnit.Value);
							bricks.Add(new TooltipBrickText(UIUtilityTexts.UpdateDescriptionWithUIProperties(fullDescription, null)));
							if (entityFact != null)
							{
								m_PreviewUnit.Value.Facts.Remove(entityFact);
							}
						}
					}
				}
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
		CreatePrerequisites(list);
		return list;
	}

	private void CreatePrerequisites(List<ITooltipBrick> result)
	{
		if (Prerequisite != null)
		{
			List<PrerequisiteEntryVM> prerequisiteEntries = UIUtility.GetPrerequisiteEntries(Prerequisite);
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			TooltipPrerequisitesUtils.AddPrerequisiteGroup(prerequisiteEntries, result, isOr: false, showAnd: true);
		}
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type != 0 && Game.Instance.IsControllerMouse && Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature.RankEntryUtils.HasPrerequisiteFooter(Prerequisite, m_Owner))
		{
			yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.PrerequisitesFooter, TooltipTitleType.H6);
		}
	}
}
