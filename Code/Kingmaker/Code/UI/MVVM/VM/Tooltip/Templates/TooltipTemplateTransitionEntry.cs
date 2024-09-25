using System.Collections.Generic;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateTransitionEntry : TooltipBaseTemplate
{
	private readonly BlueprintMultiEntranceEntry m_Entry;

	public TooltipTemplateTransitionEntry(BlueprintMultiEntranceEntry entry)
	{
		m_Entry = entry;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Entry.Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<BlueprintQuestObjective> objectives = m_Entry.GetLinkedObjectives();
		if (objectives.Count <= 0)
		{
			yield break;
		}
		yield return new TooltipBrickTitle(UIStrings.Instance.Transition.AvailableObjectives, TooltipTitleType.H3);
		foreach (BlueprintQuestObjective item in objectives)
		{
			yield return new TooltipBrickIconAndName(BlueprintRoot.Instance.UIConfig.UIIcons.QuestTypesIcons.GetQuestPaperTypeIcon(item.Quest.Type), item.Quest.Title, TooltipBrickElementType.Medium, frame: false);
		}
	}
}
