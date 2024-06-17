using System.Linq;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathList;

public class CareerPathsListsConsoleView : CareerPathsListsCommonView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavigationBehaviour?.Clear();
		m_NavigationBehaviour = null;
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	private void CreateNavigation(IConsoleNavigationOwner owner)
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		UpdateNavigation();
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			bool isFocused = m_NavigationBehaviour.IsFocused;
			m_NavigationBehaviour.Clear();
			m_NavigationBehaviour.AddColumn((from i in m_CareerPathsLists
				where i.IsBinded
				select i.GetNavigationBehaviour()).ToList());
			if (isFocused)
			{
				m_NavigationBehaviour.FocusOnCurrentEntity();
			}
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner)
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation(owner);
		}
		foreach (CareerPathsListCommonView careerPathsList in m_CareerPathsLists)
		{
			if (careerPathsList.IsBinded)
			{
				careerPathsList.UpdateCurrentEntity();
			}
		}
		return m_NavigationBehaviour;
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
	}

	private void OnConfirmClick()
	{
		m_NavigationBehaviour.OnConfirmClick();
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
	}
}
