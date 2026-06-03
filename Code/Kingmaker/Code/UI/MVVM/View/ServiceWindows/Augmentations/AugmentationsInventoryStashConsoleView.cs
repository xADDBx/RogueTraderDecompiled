using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations;

public class AugmentationsInventoryStashConsoleView : AugmentationsInventoryStashView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public GridConsoleNavigationBehaviour SlotsNavigation;

	[SerializeField]
	public AugmentationsFilterConsoleView ItemsFilter;

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		SlotsNavigation = m_ItemSlotsGroup.VirtualList.GetNavigationBehaviour();
		m_NavigationBehaviour.AddEntityVertical(SlotsNavigation);
		m_NavigationBehaviour.AddEntityVertical(new SimpleConsoleNavigationEntity(m_CoinsContainer, new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.Scrap, UIStrings.Instance.ShipCustomization.ScrapDescription)));
		m_NavigationBehaviour.SetCurrentEntity(SlotsNavigation);
		return m_NavigationBehaviour;
	}
}
