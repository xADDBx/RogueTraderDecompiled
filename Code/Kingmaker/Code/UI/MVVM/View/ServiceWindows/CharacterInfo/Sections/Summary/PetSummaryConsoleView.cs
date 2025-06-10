using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;

public class PetSummaryConsoleView : PetSummaryPCView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[SerializeField]
	private OwlcatMultiButton m_FocusButton;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		AddDisposable(m_NavigationBehaviour);
		base.BindViewImplementation();
		RefreshNavigation();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		RefreshNavigation();
	}

	private void RefreshNavigation()
	{
		m_NavigationBehaviour.AddColumn<OwlcatMultiButton>(m_FocusButton);
		if (m_TooltipBrickTextConsoleView.IsBinded)
		{
			m_NavigationBehaviour.Clear();
			m_NavigationBehaviour.AddColumn<IConsoleEntity>(m_TooltipBrickTextConsoleView?.GetConsoleEntity());
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
	}

	private void Scroll(InputActionEventData arg1, float value)
	{
		m_ScrollRect.Scroll(value, smooth: true);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
