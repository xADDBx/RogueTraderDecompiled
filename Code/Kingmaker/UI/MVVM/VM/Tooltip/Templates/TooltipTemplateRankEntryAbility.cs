using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRankEntryAbility : TooltipTemplateAbility
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly IReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	public TooltipTemplateRankEntryAbility(BlueprintAbility blueprintAbility, FeatureSelectionItem featureSelectionItem, IReadOnlyReactiveProperty<SelectionStateFeature> selectionState, MechanicEntity caster)
		: base(blueprintAbility, null, caster)
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
			list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.Prerequisites, TooltipTextType.BoldCentered));
			list.Add(new TooltipBrickPrerequisite(UIUtility.GetPrerequisiteEntries(calculatedPrerequisite)));
		}
		return list;
	}
}
