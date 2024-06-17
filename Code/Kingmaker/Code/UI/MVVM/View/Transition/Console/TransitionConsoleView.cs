using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Transition.Common;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Console;

public class TransitionConsoleView : TransitionBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
		BuildNavigation();
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Transition"
		});
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.EnterLocation();
		}, 8), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour.SetEntitiesVertical(GetNavigationEntities());
		SetBeamOnCurrentLocation();
		IConsoleEntity consoleEntity = m_NavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is TransitionLegendButtonView transitionLegendButtonView && transitionLegendButtonView.CheckHover());
		if (consoleEntity != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(consoleEntity);
		}
		else
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}
	}

	private List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		if (CurrentPart.WidgetList.Entries != null)
		{
			foreach (MonoBehaviour entry in CurrentPart.WidgetList.Entries)
			{
				if (entry is TransitionLegendButtonView item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private void OnConfirmClick()
	{
		(m_NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
	}
}
