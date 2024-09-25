using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickTripleTextConsoleView : TooltipBrickTripleTextView, IConsoleTooltipBrick
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_LeftMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_MiddleMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_RightMultiButton;

	private SimpleConsoleNavigationEntity m_LeftSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_MiddleSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_RightSimpleConsoleNavigationEntity;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_LeftSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_LeftMultiButton);
		m_MiddleSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_MiddleMultiButton);
		m_RightSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_RightMultiButton);
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_LeftSimpleConsoleNavigationEntity, m_MiddleSimpleConsoleNavigationEntity, m_RightSimpleConsoleNavigationEntity);
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
