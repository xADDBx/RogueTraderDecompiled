using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tutorial.Console;

public class TutorialSmallWindowConsoleView : TutorialWindowConsoleView<TutorialHintWindowVM>
{
	[Space]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private int m_ViewPortHeight = 350;

	[SerializeField]
	private LayoutElement m_ViewPort;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private ConsoleHint m_CloseWindowHint;

	[SerializeField]
	private ConsoleHint m_OptionsCloseHint;

	[SerializeField]
	private ConsoleHint m_EnterSmallTutorHintsHint;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private float m_Offset;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private bool m_IsHintsSetup;

	private readonly BoolReactiveProperty m_IsInSmallTutor = new BoolReactiveProperty();

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	protected override bool IsBigTutorial => false;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetContent();
		m_IsInSmallTutor.Value = false;
		m_ScrollRect.ScrollToTop();
		Rebind();
		m_GlossaryHint.SetActive(state: false);
		m_CloseGlossaryHint.SetActive(state: false);
		m_EncyclopediaHint.SetActive(state: false);
		m_CloseWindowHint.SetActive(state: false);
		m_EnterSmallTutorHintsHint.SetLabel(UIStrings.Instance.CommonTexts.Expand);
		m_OptionsCloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow.Text);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_Disposable.Clear();
		ExitTutorialNavigation();
	}

	private void EnterTutorialNavigation()
	{
		if (!m_IsHintsSetup)
		{
			m_IsHintsSetup = true;
			m_IsInSmallTutor.Value = true;
			CreateInput();
			base.ViewModel.ChangeExpandState();
		}
	}

	private void ExitTutorialNavigation()
	{
		m_IsInSmallTutor.Value = false;
		CloseGlossary();
		m_IsHintsSetup = false;
		DisposeInput();
		base.ViewModel.ChangeExpandState();
	}

	private void Rebind()
	{
		if (!m_IsHintsSetup)
		{
			CloseGlossary();
			InputLayer inputLayer = new InputLayer
			{
				ContextName = "TutorialHintsLayer"
			};
			m_Disposable.Clear();
			m_Disposable.Add(m_EnterSmallTutorHintsHint.Bind(inputLayer.AddButton(delegate
			{
				EnterTutorialNavigation();
			}, 16, m_IsInSmallTutor.Not().ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
			m_Disposable.Add(m_OptionsCloseHint.Bind(inputLayer.AddButton(delegate
			{
				base.ViewModel.Hide();
			}, 16, m_IsInSmallTutor.Not().ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
			GamePad.Instance.SetOverlayLayer(inputLayer);
		}
	}

	private void DisposeInput()
	{
		GamePad.Instance.SetOverlayLayer(null);
		NavigationBehaviour = null;
		GamePad.Instance.PopLayer(InputLayer);
		InputLayer = null;
	}

	private void CreateInput()
	{
		AddDisposable(NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters));
		GlossaryInputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SmallTutorGlossary"
		});
		InputLayer = new InputLayer
		{
			ContextName = "SmallTutorialWindow"
		};
		DelayedGlossaryCalculation();
		AddDisposable(m_CloseWindowHint.Bind(InputLayer.AddButton(delegate
		{
			base.ViewModel.Hide();
		}, 9, IsGlossaryMode.Not().And(m_IsInSmallTutor).ToReactiveProperty())));
		m_CloseWindowHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow.Text);
		AddDisposable(m_ToggleHint.Bind(InputLayer.AddButton(base.SelectDeselectToggle, 10, IsGlossaryMode.Not().And(m_IsInSmallTutor).ToReactiveProperty())));
		AddDisposable(m_GlossaryHint.Bind(InputLayer.AddButton(delegate
		{
			ShowGlossary();
		}, 11, IsGlossaryMode.Not().And(HasGlossaryPoints).And(m_IsInSmallTutor)
			.ToReactiveProperty())));
		m_GlossaryHint.SetLabel(UIStrings.Instance.Dialog.OpenGlossary);
		AddDisposable(m_CloseGlossaryHint.Bind(GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, IsGlossaryMode)));
		m_CloseGlossaryHint.SetLabel(UIStrings.Instance.Dialog.CloseGlossary);
		AddDisposable(m_EncyclopediaHint.Bind(GlossaryInputLayer.AddButton(delegate
		{
			GoToEncyclopedia();
		}, 10, HasGlossaryPoints.And(IsGlossaryMode.And(IsPossibleGoToEncyclopedia)).And(m_IsInSmallTutor).ToReactiveProperty())));
		m_EncyclopediaHint.SetLabel(UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton);
		AddDisposable(InputLayer.AddAxis(Scroll, 3, IsGlossaryMode.Not().ToReactiveProperty()));
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	protected override void OnFocusLink(string key)
	{
		IsPossibleGoToEncyclopedia.Value = TooltipHelper.GetLinkTooltipTemplate(key) is TooltipTemplateGlossary;
		LinkKey = key;
		if (!(this == null))
		{
			StartCoroutine(DelayedEnsureVisible());
		}
	}

	protected override void Focus()
	{
		if (IsGlossaryMode.Value)
		{
			m_FirstGlossaryFocus.ShowTooltip(TooltipHelper.GetLinkTooltipTemplate(LinkKey), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace), null, NavigationBehaviour);
		}
	}

	private void DelayedFocus()
	{
		DelayedInvoker.InvokeInFrames(Focus, 1);
	}

	protected override void OnShow()
	{
		base.OnShow();
		UISounds.Instance.Sounds.Tutorial.ShowSmallTutorial.Play();
		StartCoroutine(SetSizeDelayed());
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_IsHintsSetup = false;
		CloseGlossary();
		UISounds.Instance.Sounds.Tutorial.HideSmallTutorial.Play();
		base.ViewModel.ChangeExpandState();
	}

	private void SetContent()
	{
		SetPage(base.ViewModel.Pages.FirstOrDefault());
	}

	private void SetWindowSize()
	{
		m_ViewPort.preferredHeight = Mathf.Min(m_ViewPortHeight, m_Content.sizeDelta.y + m_Offset);
	}

	private IEnumerator SetSizeDelayed()
	{
		yield return new WaitForEndOfFrame();
		SetWindowSize();
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
}
