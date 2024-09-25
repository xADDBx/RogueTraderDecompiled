using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class CombinedTooltipData : TooltipData
{
	public readonly List<TooltipBaseTemplate> Templates;

	public CombinedTooltipData(List<TooltipBaseTemplate> templates, TooltipConfig config, ReactiveCommand closeCommand = null, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
		: base(templates.FirstOrDefault(), config, closeCommand, ownerNavigationBehaviour)
	{
		Templates = templates;
	}
}
