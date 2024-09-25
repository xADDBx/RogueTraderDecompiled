using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsConsoleView : CreditsBaseView
{
	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_PlayRolesHint;

	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_HasSearchResults = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_InputFieldIsFocused = new BoolReactiveProperty();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ConsoleInputField.Bind(UIStrings.Instance.Credits.EnterSearchNameHere, delegate
		{
			OnFind();
			BoolReactiveProperty hasSearchResults = m_HasSearchResults;
			int value;
			if (!string.IsNullOrEmpty(m_SearchField.text))
			{
				LinkedList<PageGenerator.SearchResult> resultSearch = ResultSearch;
				value = ((resultSearch != null && resultSearch.Count > 0) ? 1 : 0);
			}
			else
			{
				value = 0;
			}
			hasSearchResults.Value = (byte)value != 0;
		});
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
	}

	protected override void DestroyViewImplementation()
	{
		m_InputFieldIsFocused.Value = false;
		m_HasSearchResults.Value = false;
		base.DestroyViewImplementation();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CreditsView"
		});
		AddDisposable(m_PrevHint.Bind(m_InputLayer.AddButton(delegate
		{
			ChangeTab(direction: false);
		}, 14)));
		AddDisposable(m_NextHint.Bind(m_InputLayer.AddButton(delegate
		{
			ChangeTab(direction: true);
		}, 15)));
		AddDisposable(m_PlayRolesHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.TogglePause();
		}, 11)));
		AddDisposable(base.ViewModel.Pause.Subscribe(RefreshPlayRolesLabel));
		RefreshPlayRolesLabel(base.ViewModel.Pause.Value);
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.CloseCredits();
		}, 9, m_InputFieldIsFocused.Not().ToReactiveProperty()), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(!m_InputFieldIsFocused.Value);
		}, 9, m_InputFieldIsFocused), UIStrings.Instance.SettingsUI.Cancel));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnPrevPage(delegate
			{
				ChangeTab(direction: false);
			});
		}, 4, InputActionEventType.ButtonRepeating), UIStrings.Instance.Credits.PreviousPage, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 14), UIStrings.Instance.Credits.PreviousGroup, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
		}, 15), UIStrings.Instance.Credits.NextGroup, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnNextPage(delegate
			{
				ChangeTab(direction: true);
			});
		}, 5, InputActionEventType.ButtonRepeating), UIStrings.Instance.Credits.NextPage, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnFind();
			ActivateDeactivateInputField(state: false);
		}, 8, base.ViewModel.InputFieldHasAnySymbol), UIStrings.Instance.CommonTexts.Search));
		string label = UIStrings.Instance.Credits.EnterSearchNameHere.Text.TrimEnd('.');
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ActivateDeactivateInputField(!m_InputFieldIsFocused.Value);
		}, 10, InputActionEventType.ButtonJustReleased), label));
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_PlayRolesHint.transform.SetAsFirstSibling();
		}, 1);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void ActivateDeactivateInputField(bool state)
	{
		if (state)
		{
			m_ConsoleInputField.Select();
		}
		else
		{
			m_ConsoleInputField.Abort();
		}
		m_InputFieldIsFocused.Value = state;
	}

	private void RefreshPlayRolesLabel(bool state)
	{
		m_PlayRolesHint.SetLabel(state ? UIStrings.Instance.Credits.PlayRoles : UIStrings.Instance.CommonTexts.Pause);
	}
}
