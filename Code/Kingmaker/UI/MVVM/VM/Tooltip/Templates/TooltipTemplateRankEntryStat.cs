using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRankEntryStat : TooltipTemplateStat
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly IReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	public TooltipTemplateRankEntryStat(StatTooltipData statData, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, bool showCompanionStats = false)
		: base(statData, showCompanionStats)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = base.GetBody(type).ToList();
		CalculatedPrerequisite calculatedPrerequisite = m_SelectionState.Value?.GetCalculatedPrerequisite(m_SelectionItem);
		if (calculatedPrerequisite != null)
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			list.Add(new TooltipBrickPrerequisite(UIUtility.GetPrerequisiteEntries(calculatedPrerequisite), oneFromList: false));
		}
		return list;
	}
}
