using System.Linq;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickWeaponSetConsoleView : TooltipBrickWeaponSetView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		SimpleConsoleNavigationEntity entity = new SimpleConsoleNavigationEntity(m_HandSlotView.Slot, m_HandSlotView.TooltipTemplates().LastOrDefault());
		m_NavigationBehaviour.AddEntityVertical(entity);
		if (m_WidgetList.Entries != null)
		{
			m_NavigationBehaviour.AddRow(m_WidgetList.Entries.Select((IWidgetView e) => (e as CharInfoWeaponSetAbilityPCView)?.NavigationEntity).ToList());
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
