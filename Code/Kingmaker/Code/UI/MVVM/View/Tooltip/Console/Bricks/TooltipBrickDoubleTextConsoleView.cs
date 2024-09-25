using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickDoubleTextConsoleView : TooltipBrickDoubleTextView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_LeftMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_RightMultiButton;

	private SimpleConsoleNavigationEntity m_LeftSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_RightSimpleConsoleNavigationEntity;

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
		m_LeftSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_LeftMultiButton);
		m_RightSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_RightMultiButton);
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_LeftSimpleConsoleNavigationEntity, m_RightSimpleConsoleNavigationEntity);
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
