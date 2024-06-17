using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickArmorStatsConsoleView : TooltipBrickArmorStatsView, IConsoleTooltipBrick, IMonoBehaviour
{
	[SerializeField]
	private OwlcatMultiButton m_DeflectionButton;

	[SerializeField]
	private OwlcatMultiButton m_AbsorptionButton;

	[SerializeField]
	private OwlcatMultiButton m_DodgeButton;

	private SimpleConsoleNavigationEntity m_DeflectionNavigationEntity;

	private SimpleConsoleNavigationEntity m_AbsorptionNavigationEntity;

	private SimpleConsoleNavigationEntity m_DodgeNavigationEntity;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_DeflectionNavigationEntity = new SimpleConsoleNavigationEntity(m_DeflectionButton);
		m_AbsorptionNavigationEntity = new SimpleConsoleNavigationEntity(m_AbsorptionButton);
		m_DodgeNavigationEntity = new SimpleConsoleNavigationEntity(m_DodgeButton);
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_DeflectionNavigationEntity, m_AbsorptionNavigationEntity, m_DodgeNavigationEntity);
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
