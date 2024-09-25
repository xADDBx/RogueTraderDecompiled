using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.BookEvent;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

[RequireComponent(typeof(DialogColorsConfig))]
public class BookEventBaseView : ViewBase<BookEventVM>, IEncyclopediaGlossaryModeHandler, ISubscriber, IInitializable
{
	[Header("Window")]
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[SerializeField]
	private FadeAnimator m_ContentAnimator;

	[Header("Text page")]
	[SerializeField]
	private GameObject m_CuesPanel;

	[SerializeField]
	protected BookEventCueView m_CueView;

	private readonly List<CueVM> m_MemorizedCues = new List<CueVM>();

	protected readonly List<BookEventCueView> MemorizedCuesViews = new List<BookEventCueView>();

	protected readonly List<BookEventCueView> CurrentCuesViews = new List<BookEventCueView>();

	[SerializeField]
	protected ScrollRectExtended m_CuesScrollRect;

	[SerializeField]
	private GameObject m_AnswersPanel;

	[SerializeField]
	private BookEventAnswerView m_AnswerView;

	[SerializeField]
	protected ScrollRectExtended m_AnswersScrollRect;

	private bool m_ContentRefreshing;

	[Header("Picture page")]
	[SerializeField]
	private Image m_Picture;

	[SerializeField]
	private FadeAnimator m_PictureAnimator;

	[Header("Notification")]
	[SerializeField]
	protected DialogNotificationsPCView m_DialogNotifications;

	[Header("Character Select")]
	[SerializeField]
	private BookEventChooseCharacterPCView m_BookEventChooseCharacterView;

	[Header("History")]
	[SerializeField]
	private TextMeshProUGUI m_HistoryLabel;

	[SerializeField]
	protected OwlcatMultiButton m_OpenHistoryButton;

	[SerializeField]
	protected OwlcatMultiButton m_CloseHistoryButton;

	[SerializeField]
	protected OwlcatMultiButton m_SwitchHistoryButton;

	[SerializeField]
	protected Animator m_SwitchAnimator;

	[SerializeField]
	private GameObject m_CuesHistoryPanel;

	[SerializeField]
	private GameObject m_CuesHistory;

	[SerializeField]
	private GameObject m_Cues;

	[SerializeField]
	protected ScrollRectExtended m_CuesHistoryScrollRect;

	[SerializeField]
	private GameObject m_Fade;

	[Header("Mirror Block ")]
	[SerializeField]
	private Image m_MirrorPortrait;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer Layer;

	protected readonly List<BookEventAnswerView> AnswersEntities = new List<BookEventAnswerView>();

	private List<IConsoleEntity> m_ButtonsEntities = new List<IConsoleEntity>();

	private DialogColors m_DialogColors;

	private bool m_IsInit;

	private bool m_IsShown;

	private int m_LastHistoryCueIndex;

	private string m_ChoosedAnswer;

	private string m_PreviousChoosedAnswer;

	private static readonly int SwitchToRight = Animator.StringToHash("SwitchToRight");

	protected readonly BoolReactiveProperty VotesIsActive = new BoolReactiveProperty();

	public BoolReactiveProperty IsShowHistory { get; } = new BoolReactiveProperty();


	public virtual void Initialize()
	{
		m_DialogColors = GetComponent<DialogColorsConfig>().DialogColors;
		m_WindowAnimator.Initialize();
		m_ContentAnimator.Initialize();
		m_PictureAnimator.Initialize();
		m_DialogNotifications.Initialize(m_DialogColors);
		m_BookEventChooseCharacterView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_LastHistoryCueIndex = -1;
		SetLocalizedLabels();
		Show();
		m_DialogNotifications.Bind(base.ViewModel.DialogNotifications);
		CreateNavigation();
		AddDisposable(base.ViewModel.EventPicture.Select((Sprite ev) => ev != null).Subscribe(delegate
		{
			SetupPicture();
		}));
		AddDisposable(base.ViewModel.Cues.ObserveAdd().Subscribe(delegate
		{
			OnContentChanged();
		}));
		AddDisposable(base.ViewModel.Answers.Subscribe(delegate(List<AnswerVM> vms)
		{
			if (vms != null)
			{
				OnContentChanged();
			}
		}));
		AddDisposable(base.ViewModel.ChooseCharacterActive.Subscribe(OnChooseCharacterState));
		AddDisposable(base.ViewModel.Mirror.Subscribe(delegate(Sprite value)
		{
			m_MirrorPortrait.sprite = value;
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_OpenHistoryButton.OnConfirmClickAsObservable(), delegate
		{
			DelayedShowHistory();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_CloseHistoryButton.OnConfirmClickAsObservable(), delegate
		{
			DelayedHideHistory();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_SwitchHistoryButton.OnConfirmClickAsObservable(), delegate
		{
			DelayedSwitchHistory();
		}));
		AddDisposable(m_OpenHistoryButton.SetHint(UIStrings.Instance.BookEvent.BookEventOpenHistory, "OpenHistory"));
		AddDisposable(m_CloseHistoryButton.SetHint(UIStrings.Instance.BookEvent.BookEventCloseHistory, "CloseHistory"));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			if (m_IsShown != value)
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}));
		AddDisposable(base.ViewModel.ChoosedAnswer.Subscribe(delegate(string value)
		{
			m_ChoosedAnswer = value;
		}));
		AddDisposable(GamePad.Instance.PushLayer(Layer));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.CheckVotesActive, delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				VotesIsActive.Value = AnswersEntities.Any((BookEventAnswerView a) => a.AnswerVotes.Any());
			}, 1);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void Show()
	{
		VotesIsActive.Value = false;
		m_IsShown = true;
		m_WindowAnimator.AppearAnimation();
		UISounds.Instance.Sounds.Dialogue.BookOpen.Play();
		m_CloseHistoryButton.SetActiveLayer("Active");
	}

	private void Hide()
	{
		VotesIsActive.Value = false;
		m_IsShown = false;
		m_WindowAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.Dialogue.BookClose.Play();
	}

	private void OnChooseCharacterState(bool state)
	{
		if (state)
		{
			m_BookEventChooseCharacterView.Bind(base.ViewModel.BookEventChooseCharacter);
		}
	}

	protected virtual void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		Layer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "BookEvent"
		});
		CreateInputImpl(Layer, m_NavigationBehaviour);
	}

	private void SetCues()
	{
		m_CuesScrollRect.ScrollToTop();
		CurrentCuesViews.Clear();
		foreach (CueVM cue in base.ViewModel.Cues)
		{
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_DialogColors);
			cueView.Bind(cue);
			cueView.transform.SetParent(m_CuesPanel.transform, worldPositionStays: false);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()}";
			cueView.m_CueGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
			if (m_MemorizedCues.Any((CueVM c) => c.RawText == cue.RawText))
			{
				cueView.Shade();
			}
			CurrentCuesViews.Add(cueView);
		}
	}

	private void SetAnswers()
	{
		VotesIsActive.Value = false;
		if (base.ViewModel.Answers.Value == null)
		{
			return;
		}
		m_AnswersScrollRect.ScrollToTop();
		AnswersEntities.Clear();
		foreach (AnswerVM item in base.ViewModel.Answers.Value)
		{
			BookEventAnswerView widget = WidgetFactory.GetWidget(m_AnswerView, activate: true, strictMatching: true);
			widget.Initialize(this);
			widget.Bind(item);
			widget.transform.SetParent(m_AnswersPanel.transform, worldPositionStays: false);
			widget.name = $"Answer {widget.transform.GetSiblingIndex()}";
			AnswersEntities.Add(widget);
		}
		UpdateNavigation(showHistory: false);
	}

	private void UpdateNavigation(bool showHistory)
	{
		m_NavigationBehaviour.Clear();
		if (!showHistory)
		{
			m_NavigationBehaviour.AddColumn(AnswersEntities.Where((BookEventAnswerView a) => a.Button.Interactable).ToList());
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected virtual void UpdateFocusLinks()
	{
	}

	public void SetAnswerFocusTo(BookEventAnswerView answer)
	{
		if ((bool)AnswersEntities.FirstOrDefault((BookEventAnswerView a) => a == answer))
		{
			m_NavigationBehaviour.FocusOnEntityManual(answer);
		}
	}

	private void FillHistory()
	{
		if (m_LastHistoryCueIndex != -1)
		{
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_DialogColors);
			cueView.transform.SetParent(m_CuesHistoryPanel.transform, worldPositionStays: false);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()} (Answer)";
			cueView.SetText("\n<b>--- " + m_ChoosedAnswer + "</b>\n ");
			cueView.Highlight();
		}
		for (int i = m_LastHistoryCueIndex + 1; i < base.ViewModel.HistoryCues.Count; i++)
		{
			CueVM cueVM = base.ViewModel.HistoryCues[i];
			BookEventCueView cueView = WidgetFactory.GetWidget(m_CueView, activate: true, strictMatching: true);
			cueView.Initialize(delegate
			{
				WidgetFactory.DisposeWidget(cueView);
			}, m_DialogColors);
			cueView.Bind(cueVM);
			cueView.transform.SetParent(m_CuesHistoryPanel.transform, worldPositionStays: false);
			cueView.name = $"Cue {cueView.transform.GetSiblingIndex()}";
			cueView.m_CueGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
			m_MemorizedCues.Add(cueVM);
			MemorizedCuesViews.Add(cueView);
			m_LastHistoryCueIndex = i;
		}
	}

	protected void ShowHistory()
	{
		m_Cues.gameObject.SetActive(value: false);
		m_CuesHistory.gameObject.SetActive(value: true);
		m_AnswersPanel.SetActive(value: false);
		m_OpenHistoryButton.SetActiveLayer("Active");
		m_CloseHistoryButton.SetActiveLayer("Normal");
		foreach (BookEventAnswerView answersEntity in AnswersEntities)
		{
			answersEntity.gameObject.SetActive(value: false);
		}
		m_Fade.SetActive(value: false);
		m_SwitchAnimator.SetBool(SwitchToRight, value: true);
		IsShowHistory.Value = true;
		UpdateNavigation(showHistory: true);
		m_CuesHistoryScrollRect.ScrollToBottom();
	}

	protected void HideHistory()
	{
		m_Cues.gameObject.SetActive(value: true);
		m_CuesHistory.gameObject.SetActive(value: false);
		m_AnswersPanel.SetActive(value: true);
		m_OpenHistoryButton.SetActiveLayer("Normal");
		m_CloseHistoryButton.SetActiveLayer("Active");
		foreach (BookEventAnswerView answersEntity in AnswersEntities)
		{
			answersEntity.gameObject.SetActive(value: true);
		}
		m_Fade.SetActive(value: true);
		m_SwitchAnimator.SetBool(SwitchToRight, value: false);
		IsShowHistory.Value = false;
		UpdateNavigation(showHistory: false);
	}

	protected void SwitchHistory()
	{
		if (IsShowHistory.Value)
		{
			HideHistory();
		}
		else
		{
			ShowHistory();
		}
		DelayedInvoker.InvokeInFrames(UpdateFocusLinks, 1);
	}

	private void OnContentChanged()
	{
		if (!m_ContentRefreshing)
		{
			if (IsShowHistory.Value)
			{
				SwitchHistory();
			}
			m_ContentRefreshing = true;
			TooltipHelper.HideTooltip();
			m_ContentAnimator.DisappearAnimation(delegate
			{
				UISounds.Instance.Sounds.Dialogue.BookPageTurn.Play();
				m_ContentAnimator.AppearAnimation();
				SetCues();
				SetAnswers();
				FillHistory();
				DelayedInvoker.InvokeInFrames(UpdateFocusLinks, 1);
				m_ContentRefreshing = false;
			});
		}
	}

	private void SetupPicture()
	{
		if (!m_PictureAnimator.gameObject.activeSelf)
		{
			SetPicture();
		}
		else
		{
			m_PictureAnimator.DisappearAnimation(SetPicture);
		}
	}

	private void SetPicture()
	{
		m_Picture.sprite = base.ViewModel.EventPicture.Value;
		m_PictureAnimator.AppearAnimation();
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, GridConsoleNavigationBehaviour behaviour)
	{
		AddDisposable(inputLayer.AddButton(OnConfirmClick, 8));
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		(m_NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (!IsShowHistory.Value)
		{
			m_CuesScrollRect.Scroll(value, smooth: true);
		}
		else
		{
			m_CuesHistoryScrollRect.Scroll(value, smooth: true);
		}
	}

	private void SetLocalizedLabels()
	{
		if ((bool)m_HistoryLabel)
		{
			m_HistoryLabel.text = UIStrings.Instance.BookEvent.BookEventArchive;
		}
	}

	private void DelayedShowHistory()
	{
		DelayedInvoker.InvokeInFrames(ShowHistory, 2);
	}

	private void DelayedHideHistory()
	{
		DelayedInvoker.InvokeInFrames(HideHistory, 2);
	}

	private void DelayedSwitchHistory()
	{
		DelayedInvoker.InvokeInFrames(SwitchHistory, 2);
	}

	public void HandleGlossaryMode(bool state)
	{
		if (!state)
		{
			OnCloseGlossaryMode();
		}
	}

	protected virtual void OnCloseGlossaryMode()
	{
	}
}
