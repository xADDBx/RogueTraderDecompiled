using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Enums;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateFactionReputationLevel : TooltipBaseTemplate
{
	[CanBeNull]
	public FactionType FactionType;

	[CanBeNull]
	public string Status;

	public TooltipTemplateFactionReputationLevel(FactionType factionType, string status)
	{
		FactionType = factionType;
		Status = status;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.CommonTexts.Information, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickFactionStatus(UIConfig.Instance.UIIcons.GetFactionIcon(FactionType), UIStrings.Instance.CharacterSheet.GetFactionLabel(FactionType), Status)
		};
	}
}
