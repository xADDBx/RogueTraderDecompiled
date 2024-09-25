using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.BuffsAndConditions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary.Console;

public class CharInfoStatusEffectsConsoleView : CharInfoStatusEffectsView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
		base.BindViewImplementation();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		if (m_WidgetList.Entries == null)
		{
			return;
		}
		foreach (IWidgetView entry in m_WidgetList.Entries)
		{
			m_NavigationBehaviour.AddRow<StatusEffectConsoleView>(entry as StatusEffectConsoleView);
		}
	}

	private void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_Scroll.EnsureVisibleVertical(targetRect);
		}
	}

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		navigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationBehaviour);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
