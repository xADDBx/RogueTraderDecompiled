using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.View.Common.InputField;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Console;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.Code.UI.MVVM.View.Settings.Console;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.EscMenu;
using Kingmaker.UI.MVVM.View.NetLobby.Console;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.MVVM.View.Tutorial.Console;

public class TutorialBigWindowConsoleView : TutorialWindowConsoleView<TutorialModalWindowVM>
{
	[Space]
	[SerializeField]
	private GameObject m_PagesBlock;

	[SerializeField]
	private PageNavigationConsole m_PageNavigation;

	[SerializeField]
	private TextMeshProUGUI m_PageNavigationText;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected TextMeshProUGUI m_ConfirmButtonText;

	[Space]
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_CloseWindowHint;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	public static readonly string InputLayerContextName = "BigTutorialWindow";

	public static readonly string GlossaryContextName = "BigTutorGlossary";

	protected override bool IsShowDefaultSprite => true;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CurrentPage.Subscribe(base.SetPage));
		m_PagesBlock.SetActive(base.ViewModel.MultiplePages);
		m_PageNavigation.Initialize(base.ViewModel.PageCount, base.ViewModel.CurrentPageIndex);
		AddDisposable(m_PageNavigation);
		CreateInput();
		AddDisposable(base.ViewModel.CurrentPage.Subscribe(delegate
		{
			DelayedGlossaryCalculation();
		}));
		AddDisposable(base.ViewModel.CurrentPageIndex.Subscribe(delegate
		{
			m_PageNavigationText.text = base.ViewModel.CurrentPageIndex.Value + 1 + "/" + base.ViewModel.PageCount;
			m_ConfirmButtonText.text = ((base.ViewModel.CurrentPageIndex.Value + 1 < base.ViewModel.PageCount) ? UIStrings.Instance.Tutorial.Next.Text : UIStrings.Instance.Tutorial.Complete.Text);
		}));
		TooltipHelper.HideTooltip();
	}

	private void CreateInput()
	{
		GamePad.Instance.BaseLayer?.Unbind();
		AddDisposable(NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		InputLayer = new InputLayer
		{
			ContextName = InputLayerContextName
		};
		GlossaryInputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = GlossaryContextName
		});
		m_PageNavigation.AddInput(InputLayer, IsGlossaryMode.Not().ToReactiveProperty(), addDpad: true, showHints: false);
		InputBindStruct inputBindStruct = InputLayer.AddButton(base.SelectDeselectToggle, 10, IsGlossaryMode.Not().ToReactiveProperty());
		AddDisposable(m_ToggleHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = InputLayer.AddButton(delegate
		{
			OnPrev();
		}, 9, m_PageNavigation.HasPrevious, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_PreviousHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_PreviousHint.SetLabel(UIStrings.Instance.Tutorial.Previous.Text);
		InputBindStruct inputBindStruct3 = InputLayer.AddButton(delegate
		{
			OnNext();
		}, 8, IsGlossaryMode.Not().ToReactiveProperty());
		AddDisposable(m_ConfirmHint.Bind(inputBindStruct3));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = InputLayer.AddButton(delegate
		{
			base.ViewModel.Hide();
		}, 9, IsGlossaryMode.Not().ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed);
		AddDisposable(m_CloseWindowHint.Bind(inputBindStruct4));
		AddDisposable(inputBindStruct4);
		m_CloseWindowHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow.Text);
		InputBindStruct inputBindStruct5 = InputLayer.AddButton(delegate
		{
			ShowGlossary();
		}, 11, IsGlossaryMode.Not().And(HasGlossaryPoints).ToReactiveProperty());
		AddDisposable(m_GlossaryHint.Bind(inputBindStruct5));
		AddDisposable(inputBindStruct5);
		m_GlossaryHint.SetLabel(UIStrings.Instance.Dialog.OpenGlossary.Text);
		InputBindStruct inputBindStruct6 = GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, IsGlossaryMode, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_CloseGlossaryHint.Bind(inputBindStruct6));
		AddDisposable(inputBindStruct6);
		m_CloseGlossaryHint.SetLabel(UIStrings.Instance.Dialog.CloseGlossary.Text);
		InputBindStruct inputBindStruct7 = GlossaryInputLayer.AddButton(delegate
		{
			GoToEncyclopedia();
		}, 10);
		AddDisposable(m_EncyclopediaHint.Bind(inputBindStruct7));
		AddDisposable(inputBindStruct7);
		m_EncyclopediaHint.SetLabel(UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton.Text);
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = IsGlossaryMode.Not().ToReactiveProperty();
		AddDisposable(InputLayer.AddAxis(Scroll, 3, readOnlyReactiveProperty));
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
		AddDisposable(GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged));
	}

	private void OnNext()
	{
		if (base.ViewModel.CurrentPageIndex.Value < base.ViewModel.PageCount - 1)
		{
			m_PageNavigation.OnNextClick();
			UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
		}
		else
		{
			base.ViewModel.Hide();
		}
	}

	private void OnPrev()
	{
		m_PageNavigation.OnPreviousClick();
		UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
	}

	protected override void OnFocusLink(string key)
	{
		LinkKey = key;
		StartCoroutine(DelayedEnsureVisible());
	}

	protected override void GoToEncyclopedia()
	{
		if (IsGlossaryMode.Value)
		{
			base.GoToEncyclopedia();
		}
		else
		{
			base.ViewModel.GoToEncyclopedia();
		}
	}

	protected override void Focus()
	{
		if (IsGlossaryMode.Value)
		{
			OwlcatMultiButton firstGlossaryFocus = m_FirstGlossaryFocus;
			TooltipBaseTemplate linkTooltipTemplate = TooltipHelper.GetLinkTooltipTemplate(LinkKey);
			ConsoleNavigationBehaviour navigationBehaviour = NavigationBehaviour;
			firstGlossaryFocus.ShowTooltip(linkTooltipTemplate, default(TooltipConfig), null, navigationBehaviour);
		}
	}

	private void DelayedFocus()
	{
		DelayedInvoker.InvokeInFrames(Focus, 1);
	}

	private IEnumerator DelayedEnsureVisible()
	{
		yield return new WaitForEndOfFrame();
		FocusAndEnsureVisible();
	}

	private void FocusAndEnsureVisible()
	{
		DelayedFocus();
		EnsureVisible();
	}

	private void EnsureVisible()
	{
		m_ScrollRect.EnsureVisibleVertical(m_FirstGlossaryFocus.transform as RectTransform);
	}

	public void Scroll(InputActionEventData data, float x)
	{
		if (!(m_ScrollRect == null))
		{
			PointerEventData data2 = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity)
			};
			m_ScrollRect.OnSmoothlyScroll(data2);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		TooltipHelper.HideTooltip();
		UISounds.Instance.Sounds.Tutorial.ShowBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		CloseGlossary();
		GamePad.Instance.BaseLayer?.Bind();
		UISounds.Instance.Sounds.Tutorial.HideBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != InputLayer && instance.CurrentInputLayer != GlossaryInputLayer && !(instance.CurrentInputLayer.ContextName == InfoWindowConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == "SaveLoad") && !(instance.CurrentInputLayer.ContextName == "SaveFullScreenshotConsoleView") && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == ContextMenuConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == EscMenuBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == SettingsConsoleView.SettingsInputLayerName) && !(instance.CurrentInputLayer.ContextName == SettingsConsoleView.GlossarySettingsInputLayerName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			CloseGlossary();
			instance.PopLayer(InputLayer);
			instance.PushLayer(InputLayer);
		}
	}
}
