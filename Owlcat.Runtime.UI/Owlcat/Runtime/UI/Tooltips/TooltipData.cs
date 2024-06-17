using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Owlcat.Runtime.UI.Tooltips;

public class TooltipData
{
	private readonly TooltipBaseTemplate m_Template;

	public readonly TooltipConfig Config;

	public readonly ReactiveCommand CloseCommand;

	public ConsoleNavigationBehaviour OwnerNavigationBehaviour;

	public TooltipBaseTemplate MainTemplate => m_Template;

	public TooltipData(TooltipBaseTemplate template, TooltipConfig config, ReactiveCommand closeCommand = null, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		m_Template = template;
		Config = config;
		CloseCommand = closeCommand;
		OwnerNavigationBehaviour = ownerNavigationBehaviour;
	}
}
