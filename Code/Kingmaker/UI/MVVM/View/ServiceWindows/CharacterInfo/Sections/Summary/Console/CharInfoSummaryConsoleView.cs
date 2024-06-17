using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary.Console;

public class CharInfoSummaryConsoleView : CharInfoSummaryPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private SimpleConsoleNavigationEntity m_MovePointsConsoleEntity;

	private SimpleConsoleNavigationEntity m_ActionPointsConsoleEntity;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharScreenSummaryView"
		});
		m_MovePointsConsoleEntity = new SimpleConsoleNavigationEntity(m_MovePointsButton);
		m_ActionPointsConsoleEntity = new SimpleConsoleNavigationEntity(m_ActionPointsButton);
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		m_NavigationBehaviour.Clear();
		m_AlignmentWheelPCView.AddInput(ref m_InputLayer, ref m_NavigationBehaviour, null);
		GridConsoleNavigationBehaviour navigationBehaviour2 = new GridConsoleNavigationBehaviour();
		(m_StatusEffectsView as ICharInfoComponentConsoleView)?.AddInput(ref inputLayer, ref navigationBehaviour2, hintsWidget);
		navigationBehaviour2.AddRow<SimpleConsoleNavigationEntity>(m_MovePointsConsoleEntity);
		navigationBehaviour2.AddRow<SimpleConsoleNavigationEntity>(m_ActionPointsConsoleEntity);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour2);
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
