using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Console;
using Kingmaker.Code.UI.MVVM.View.MainMenu.Common;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.MainMenu.Console;

public class MainMenuSideBarConsoleView : MainMenuSideBarView<ContextMenuEntityConsoleView>, ISavesUpdatedHandler, ISubscriber
{
	[Space]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_LicenseHint;

	[SerializeField]
	private OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	private OwlcatMultiButton m_SecondGlossaryFocus;

	[Header("XBox")]
	[SerializeField]
	protected GameObject m_XBoxGamerGroup;

	[SerializeField]
	protected TextMeshProUGUI m_XBoxGamerTagText;

	[SerializeField]
	protected RawImage m_XBoxGamerRawImage;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_LinkNavigation;

	private InputLayer m_LinkInputLayer;

	private readonly BoolReactiveProperty m_InputEnabled = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasLinks = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsLinkMode = new BoolReactiveProperty();

	[Header("Navigation")]
	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	private FloatConsoleNavigationBehaviour NavigationBehaviour { get; set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_XBoxGamerGroup.gameObject.SetActive(value: false);
		BuildNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		NavigationBehaviour.Clear();
		NavigationBehaviour = null;
		m_InputLayer = null;
		m_LinkNavigation.Clear();
		m_LinkNavigation = null;
		m_LinkInputLayer = null;
		m_HintsWidget.Dispose();
	}

	private void BuildNavigation()
	{
		FloatConsoleNavigationBehaviour disposable = (NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters));
		AddDisposable(disposable);
		List<ContextMenuEntityConsoleView> list = new List<ContextMenuEntityConsoleView> { m_ContinueView, m_NewGameView, m_LoadView, m_OptionsView, m_CreditView };
		list.Add(m_NetView);
		if (base.ViewModel.ExitEnabled)
		{
			list.Add(m_ExitView);
		}
		NavigationBehaviour.AddEntities(list);
		NavigationBehaviour.FocusOnEntityManual(m_ContinueView.IsValid() ? m_ContinueView : m_NewGameView);
		m_InputLayer = GetInputLayer();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 3);
		AddDisposable(EventBus.Subscribe(this));
		m_InputEnabled.Value = true;
	}

	public void OnSaveListUpdated()
	{
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "MainMenuSideBarInputContext"
		});
		AddDisposable(inputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(inputLayer.AddButton(OnConfirmClick, 8, m_InputEnabled));
		if (BuildModeUtility.IsDevelopment)
		{
			AddDisposable(inputLayer.AddButton(OnStreamSaves, 17, m_InputEnabled));
		}
		AddDisposable(m_HintsWidget.BindHint(inputLayer.AddButton(EnterLinks, 11, m_HasLinks.And(m_InputEnabled).ToReactiveProperty()), "", ConsoleHintsWidget.HintPosition.Left));
		return inputLayer;
	}

	private async void OnStreamSaves(InputActionEventData obj)
	{
		await base.ViewModel.OnStreamSaves();
	}

	private void CalculateGlossary()
	{
		AddDisposable(m_LinkNavigation = new GridConsoleNavigationBehaviour(null, null, Vector2Int.one, lineGrid: true));
		List<IFloatConsoleNavigationEntity> list = TMPLinkNavigationGenerator.GenerateEntityList(m_MotivationText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, OnClickLink, OnFocusLink, null);
		List<IConsoleNavigationEntity> entities = new List<IConsoleNavigationEntity> { m_LicenceButton, m_WebsiteButton, m_DiscordButton };
		m_LinkNavigation.SetEntitiesVertical(list);
		m_LinkNavigation.AddRow(entities);
		m_HasLinks.Value = list.Any();
		m_LinkInputLayer = m_LinkNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "MainMenuLinks"
		});
		AddDisposable(m_HintsWidget.BindHint(m_LinkInputLayer.AddButton(ExitLinks, 9, m_IsLinkMode), UIStrings.Instance.CommonTexts.Back, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(m_LinkInputLayer.AddButton(ExitLinks, 11, m_IsLinkMode));
		AddDisposable(m_HintsWidget.BindHint(m_LinkInputLayer.AddButton(delegate
		{
		}, 8, m_IsLinkMode), UIStrings.Instance.CommonTexts.Select, ConsoleHintsWidget.HintPosition.Left));
	}

	private void OnClickLink(string key)
	{
		Application.OpenURL(key);
	}

	private void OnFocusLink(string key)
	{
	}

	private void EnterLinks(InputActionEventData data)
	{
		if (m_InputEnabled.Value)
		{
			NavigationBehaviour.UnFocusCurrentEntity();
			GamePad.Instance.PushLayer(m_LinkInputLayer);
			m_LinkNavigation.FocusOnFirstValidEntity();
			m_IsLinkMode.Value = true;
		}
	}

	private void ExitLinks(InputActionEventData data)
	{
		m_LinkNavigation.UnFocusCurrentEntity();
		GamePad.Instance.PopLayer(m_LinkInputLayer);
		m_IsLinkMode.Value = false;
		NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		if (NavigationBehaviour.CurrentEntity is ContextMenuEntityConsoleView)
		{
			(NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
		}
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	protected override void UpdateMessageOfTheDay()
	{
		base.UpdateMessageOfTheDay();
		m_LinkNavigation.Clear();
		m_LinkNavigation = null;
		m_LinkInputLayer = null;
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 3);
	}
}
