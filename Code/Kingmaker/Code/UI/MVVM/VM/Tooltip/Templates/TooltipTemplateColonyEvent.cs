using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyEvent : TooltipBaseTemplate
{
	[CanBeNull]
	public string EventName { get; }

	[CanBeNull]
	public string EventDescription { get; }

	public bool IsColonyManagement { get; }

	public TooltipTemplateColonyEvent(string eventName, string eventDescription, bool isColonyManagement)
	{
		EventName = eventName;
		EventDescription = eventDescription;
		IsColonyManagement = isColonyManagement;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(EventName, TooltipTitleType.H2, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> obj = new List<ITooltipBrick>
		{
			new TooltipBrickText(EventDescription)
		};
		string text = (IsColonyManagement ? UIStrings.Instance.ColonyEventsTexts.NeedsVisitMechanicString.Text : UIStrings.Instance.ColonyEventsTexts.NeedsResolveMechanicString.Text);
		obj.Add(new TooltipBrickText("<i>" + text + "</i>"));
		return obj;
	}
}
