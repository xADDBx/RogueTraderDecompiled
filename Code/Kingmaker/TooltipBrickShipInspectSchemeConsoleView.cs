using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker;

public class TooltipBrickShipInspectSchemeConsoleView : TooltipBrickShipInspectSchemeView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_GridNavigationBehaviour;

	private Dictionary<ArmourAndShieldValueType, IConsoleEntity> m_NavigationEntities = new Dictionary<ArmourAndShieldValueType, IConsoleEntity>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_GridNavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public void CreateNavigation()
	{
		m_GridNavigationBehaviour.Clear();
		foreach (InspectSchemeValueView value in Values)
		{
			m_NavigationEntities[value.GetArmourAndShieldValue()] = value.GetConsoleEntity();
		}
		IConsoleEntity item = m_NavigationEntities[ArmourAndShieldValueType.ArmourAft];
		IConsoleEntity item2 = m_NavigationEntities[ArmourAndShieldValueType.ShieldPort];
		IConsoleEntity item3 = m_NavigationEntities[ArmourAndShieldValueType.ArmourPort];
		List<IConsoleEntity> entities = new List<IConsoleEntity> { item, item2, item3 };
		m_GridNavigationBehaviour.AddRow(entities);
		IConsoleEntity item4 = m_NavigationEntities[ArmourAndShieldValueType.ShieldAft];
		IConsoleEntity item5 = m_NavigationEntities[ArmourAndShieldValueType.ShieldFore];
		List<IConsoleEntity> entities2 = new List<IConsoleEntity> { item4, item5 };
		m_GridNavigationBehaviour.AddRow(entities2);
		IConsoleEntity item6 = m_NavigationEntities[ArmourAndShieldValueType.ArmourStarboard];
		IConsoleEntity item7 = m_NavigationEntities[ArmourAndShieldValueType.ShieldStarboard];
		IConsoleEntity item8 = m_NavigationEntities[ArmourAndShieldValueType.ArmourFore];
		List<IConsoleEntity> entities3 = new List<IConsoleEntity> { item6, item7, item8 };
		m_GridNavigationBehaviour.AddRow(entities3);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_GridNavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
