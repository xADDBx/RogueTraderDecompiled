using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.PubSubSystem;

public interface ITooltipHandler : ISubscriber
{
	void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false);

	void HandleInfoRequest(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false);

	void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null);

	void HandleGlossaryInfoRequest(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null);

	void HandleHintRequest(HintData data, bool shouldShow);

	void HandleComparativeTooltipRequest(IEnumerable<TooltipData> data, bool showScrollbar = false);
}
