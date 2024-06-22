using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;
using Kingmaker.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;

public class SurfaceDialogBaseView<TAnswerView> : ViewBase<DialogVM>, IEncyclopediaGlossaryModeHandler, ISubscriber, ICargoRewardsUIHandler, ISettingsFontSizeUIHandler, IInitializable, IHasBlueprintInfo where TAnswerView : DialogAnswerBaseView
{
	[Header("Speaker Portrait")]
	[SerializeField]
	protected TextMeshProUGUI m_SpeakerName;

	[SerializeField]
	protected Image m_SpeakerPortrait;

	[SerializeField]
	protected RectTransform m_SpeakerNoPortrait;

	[SerializeField]
	protected RectTransform m_SpeakerNoPortraitEqualizerFx;

	[SerializeField]
	protected CanvasGroup m_SpeakerGlow;

	[Header("Speaker Block")]
	[SerializeField]
	protected ScrollRectExtended m_SpeakerScrollRect;

	[SerializeField]
	protected CanvasGroup m_SpeakerContentCanvasGroup;

	[SerializeField]
	protected float m_SpeakerContentFadeTime = 0.2f;

	[SerializeField]
	protected DialogCuePCView m_CueView;

	[SerializeField]
	protected RectTransform m_CuesTooltipPlace;

	[Header("History Block")]
	[SerializeField]
	protected DialogHistoryEntity m_HistoryEntity;

	[SerializeField]
	protected RectTransform m_HistoryContainer;

	[Header("Notification Block")]
	[SerializeField]
	protected DialogNotificationsPCView m_DialogNotifications;

	[Header("Answer Portrait")]
	[SerializeField]
	protected TextMeshProUGUI m_AnswerName;

	[SerializeField]
	protected Image m_AnswererPortrait;

	[SerializeField]
	protected RectTransform m_AnswererNoPortrait;

	[SerializeField]
	protected RectTransform m_AnswerNoPortraitEqualizerFx;

	[SerializeField]
	protected CanvasGroup m_AnswererGlow;

	[Header("Answer Block")]
	[SerializeField]
	protected TAnswerView m_AnswerView;

	[SerializeField]
	protected RectTransform m_AnswerPanel;

	[SerializeField]
	protected ScrollRectExtended m_AnswerScrollRect;

	[SerializeField]
	protected CanvasGroup m_AnswerContentCanvasGroup;

	[SerializeField]
	protected FadeAnimator m_AnswerFadeAnimator;

	[FormerlySerializedAs("m_AnswererTooltipPlace")]
	[SerializeField]
	protected RectTransform m_AnswersTooltipPlace;

	[Header("Common Block")]
	[SerializeField]
	protected int m_HidePosY = -358;

	[SerializeField]
	protected int m_ShowPosY;

	[SerializeField]
	private DialogColorsConfig m_DialogConfig;

	[SerializeField]
	private AnimatorsCleaner m_Cleaner;

	[Space]
	[Header("VotesCoop")]
	[SerializeField]
	protected DialogVotesBlockView m_TopDialogVotesBlock;

	[SerializeField]
	protected DialogVotesBlockView m_BottomDialogVotesBlock;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	protected readonly List<DialogHistoryEntity> HistoryEntities = new List<DialogHistoryEntity>();

	private readonly List<IDialogShowData> m_HistoryEntitiesToAdd = new List<IDialogShowData>();

	protected GridConsoleNavigationBehaviour NavigationBehaviour;

	protected InputLayer InputLayer;

	private CanvasGroup m_CanvasGroup;

	private RectTransform m_RectTransform;

	private bool m_VisibleState;

	private DialogColors m_DialogColors;

	protected readonly List<TAnswerView> Answers = new List<TAnswerView>();

	private readonly List<AnswerVM> m_AnswerEntitiesToAdd = new List<AnswerVM>();

	private List<PlayerInfo> m_TopBoundAnswerVotes = new List<PlayerInfo>();

	private List<PlayerInfo> m_BottomBoundAnswersVotes = new List<PlayerInfo>();

	private readonly ReactiveCommand<List<PlayerInfo>> m_TopVotesShowCommand = new ReactiveCommand<List<PlayerInfo>>();

	private readonly ReactiveCommand<List<PlayerInfo>> m_BottomVotesShowCommand = new ReactiveCommand<List<PlayerInfo>>();

	protected readonly BoolReactiveProperty VotesIsActive = new BoolReactiveProperty();

	protected bool IsCargoRewardsOpen;

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : this.EnsureComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public BlueprintScriptableObject Blueprint => Game.Instance.DialogController.Dialog;

	public virtual void Initialize()
	{
		m_DialogColors = ((m_DialogConfig != null) ? m_DialogConfig.DialogColors : GetComponent<DialogColorsConfig>().DialogColors);
		m_CueView.Initialize(null, m_DialogColors, m_CuesTooltipPlace);
		base.gameObject.SetActive(value: true);
		SetVisible(state: false, force: true);
		ObjectExtensions.Or(m_PortraitFullView, null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.AnswerName.Subscribe(delegate(string value)
		{
			m_AnswerName.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_AnswerName.text = value;
			}
		}));
		AddDisposable(base.ViewModel.AnswerPortrait.Subscribe(delegate(Sprite value)
		{
			m_AnswererNoPortrait.gameObject.SetActive(value == null);
			m_AnswererPortrait.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_AnswererPortrait.sprite = value;
			}
		}));
		AddDisposable(base.ViewModel.SpeakerName.Subscribe(delegate(string value)
		{
			m_SpeakerName.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_SpeakerName.text = value;
			}
		}));
		AddDisposable(base.ViewModel.SpeakerPortrait.Subscribe(delegate(Sprite value)
		{
			m_SpeakerNoPortrait.gameObject.SetActive(value == null);
			m_SpeakerPortrait.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_SpeakerPortrait.sprite = value;
			}
		}));
		m_DialogNotifications.Bind(base.ViewModel.DialogNotifications);
		AddDisposable(base.ViewModel.Cue.Subscribe(m_CueView.Bind));
		AddDisposable(base.ViewModel.History.ObserveAdd().Subscribe(delegate(CollectionAddEvent<IDialogShowData> value)
		{
			m_HistoryEntitiesToAdd.Add(value.Value);
		}));
		AddDisposable(base.ViewModel.Answers.Where((List<AnswerVM> a) => a != null).Subscribe(m_AnswerEntitiesToAdd.AddRange));
		AddDisposable(base.ViewModel.SystemAnswer.Where((AnswerVM a) => a != null).Subscribe(m_AnswerEntitiesToAdd.Add));
		AddDisposable(base.ViewModel.IsSpeakerNeedGlow.Subscribe(delegate(bool value)
		{
			m_SpeakerGlow.DOFade(value ? 1f : 0f, value ? 0.5f : 0.1f);
		}));
		AddDisposable(base.ViewModel.IsSpeakerNeedEqualizer.Subscribe(m_SpeakerNoPortraitEqualizerFx.gameObject.SetActive));
		AddDisposable(base.ViewModel.IsAnswererNeedEqualizer.Subscribe(m_AnswerNoPortraitEqualizerFx.gameObject.SetActive));
		AddDisposable(base.ViewModel.IsAnswererNeedGlow.Subscribe(delegate(bool value)
		{
			m_AnswererGlow.DOFade(value ? 1f : 0f, value ? 0.5f : 0.1f);
		}));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			SetVisible(value);
		}));
		AddDisposable(base.ViewModel.OnCueUpdate.ObserveLastValueOnLateUpdate().Subscribe(StartUpdateCoroutine));
		StartUpdateCoroutine();
		m_TopDialogVotesBlock.Bind(base.ViewModel.DialogVotesBlockVM);
		m_BottomDialogVotesBlock.Bind(base.ViewModel.DialogVotesBlockVM);
		ObjectExtensions.Or(m_TopDialogVotesBlock, null)?.ShowHideHover(state: false);
		ObjectExtensions.Or(m_BottomDialogVotesBlock, null)?.ShowHideHover(state: false);
		AddDisposable(m_TopVotesShowCommand.Subscribe(delegate(List<PlayerInfo> value)
		{
			VotesShow(value, m_TopDialogVotesBlock);
		}));
		AddDisposable(m_BottomVotesShowCommand.Subscribe(delegate(List<PlayerInfo> value)
		{
			VotesShow(value, m_BottomDialogVotesBlock);
		}));
		if (m_PortraitFullView != null)
		{
			AddDisposable(base.ViewModel.FullPortraitVM.Subscribe(m_PortraitFullView.Bind));
		}
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.CheckVotesActive, delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				VotesIsActive.Value = Answers.Any((TAnswerView a) => a.AnswerVotes.Any());
			}, 1);
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	private void OnLateUpdate()
	{
		List<PlayerInfo> list = new List<PlayerInfo>();
		List<PlayerInfo> list2 = new List<PlayerInfo>();
		List<PlayerInfo> topBoundAnswerVotes = m_TopBoundAnswerVotes;
		List<PlayerInfo> bottomBoundAnswersVotes = m_BottomBoundAnswersVotes;
		foreach (TAnswerView item in Answers.Where((TAnswerView a) => a.AnswerVotes.Any()))
		{
			switch (m_AnswerScrollRect.VisibleVerticalPosition((RectTransform)item.transform, -10f))
			{
			case 1:
				list.AddRange(item.AnswerVotes);
				break;
			case -1:
				list2.AddRange(item.AnswerVotes);
				break;
			}
		}
		m_TopBoundAnswerVotes = list;
		m_BottomBoundAnswersVotes = list2;
		if (list.Count != topBoundAnswerVotes.Count || !list.All(topBoundAnswerVotes.Contains))
		{
			m_TopVotesShowCommand.Execute(m_TopBoundAnswerVotes);
		}
		if (list2.Count != bottomBoundAnswersVotes.Count || !list2.All(bottomBoundAnswersVotes.Contains))
		{
			m_BottomVotesShowCommand.Execute(m_BottomBoundAnswersVotes);
		}
	}

	private void VotesShow(List<PlayerInfo> players, DialogVotesBlockView votesBlock)
	{
		if (players != null && !(votesBlock == null))
		{
			votesBlock.CheckVotesPlayers(players);
		}
	}

	private void AddHistory(IDialogShowData value)
	{
		DialogHistoryEntity widget = WidgetFactory.GetWidget(m_HistoryEntity, activate: true, strictMatching: true);
		widget.Initialize(value.GetText(m_DialogColors));
		widget.transform.SetParent(m_HistoryContainer, worldPositionStays: false);
		HistoryEntities.Add(widget);
	}

	private void ClearAnswers()
	{
		ObjectExtensions.Or(m_TopDialogVotesBlock, null)?.ShowHideHover(state: false);
		ObjectExtensions.Or(m_BottomDialogVotesBlock, null)?.ShowHideHover(state: false);
		Answers.ForEach(delegate(TAnswerView a)
		{
			WidgetFactory.DisposeWidget(a);
		});
		Answers.Clear();
	}

	private void AddAnswer(AnswerVM answerVM)
	{
		TAnswerView widget = WidgetFactory.GetWidget(m_AnswerView, activate: true, strictMatching: true);
		widget.Initialize(m_DialogColors, m_AnswersTooltipPlace);
		widget.Bind(answerVM);
		widget.transform.SetParent(m_AnswerPanel, worldPositionStays: false);
		Answers.Add(widget);
	}

	private void StartUpdateCoroutine()
	{
		OnPartsUpdating();
		MainThreadDispatcher.StartUpdateMicroCoroutine(AnswerPartUpdateCoroutine());
		MainThreadDispatcher.StartUpdateMicroCoroutine(CuePartUpdateCoroutine());
	}

	protected virtual void OnPartsUpdating()
	{
	}

	protected override void DestroyViewImplementation()
	{
		HistoryEntities.ForEach(delegate(DialogHistoryEntity widget)
		{
			widget.Dispose();
			WidgetFactory.DisposeWidget(widget);
		});
		HistoryEntities.Clear();
		SetVisible(state: false, force: true);
		NavigationBehaviour?.Dispose();
		NavigationBehaviour = null;
	}

	private IEnumerator AnswerPartUpdateCoroutine()
	{
		VotesIsActive.Value = false;
		if (m_AnswerEntitiesToAdd.Empty())
		{
			yield break;
		}
		m_AnswerContentCanvasGroup.alpha = 0f;
		yield return null;
		ClearAnswers();
		foreach (AnswerVM item in m_AnswerEntitiesToAdd)
		{
			AddAnswer(item);
		}
		m_AnswerEntitiesToAdd.Clear();
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_AnswerScrollRect.viewport.transform as RectTransform);
		yield return null;
		m_AnswerScrollRect.ScrollToTop();
		yield return null;
		m_AnswerFadeAnimator.AppearAnimation();
		CreateNavigation();
	}

	private IEnumerator CuePartUpdateCoroutine()
	{
		m_SpeakerContentCanvasGroup.alpha = 0f;
		yield return null;
		foreach (IDialogShowData item in m_HistoryEntitiesToAdd)
		{
			AddHistory(item);
		}
		m_HistoryEntitiesToAdd.Clear();
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_SpeakerScrollRect.viewport.transform as RectTransform);
		yield return null;
		m_SpeakerScrollRect.ScrollToBottom();
		yield return null;
		m_SpeakerScrollRect.EnsureVisibleVertical(m_CueView.transform as RectTransform, 0f, smoothly: false, needPinch: false);
		yield return null;
		m_SpeakerContentCanvasGroup.DOFade(1f, m_SpeakerContentFadeTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		yield return null;
		m_CueView.m_CueGroup.DOFade(1f, m_SpeakerContentFadeTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
	}

	private void SetVisible(bool state, bool force = false, Action onCompleteAction = null)
	{
		if (m_VisibleState == state)
		{
			return;
		}
		VotesIsActive.Value = false;
		m_VisibleState = state;
		if (force)
		{
			CanvasGroup.alpha = (state ? 1f : 0f);
			Vector2 anchoredPosition = RectTransform.anchoredPosition;
			anchoredPosition.y = (state ? m_ShowPosY : m_HidePosY);
			RectTransform.anchoredPosition = anchoredPosition;
			onCompleteAction?.Invoke();
			m_Cleaner.SetActiveState(state);
			return;
		}
		if (state)
		{
			UISounds.Instance.Sounds.Dialogue.DialogueOpen.Play();
		}
		else
		{
			UISounds.Instance.Sounds.Dialogue.DialogueClose.Play();
		}
		CanvasGroup.DOFade(state ? 1f : 0f, m_SpeakerContentFadeTime).SetUpdate(isIndependentUpdate: true).OnStart(delegate
		{
			if (state)
			{
				m_Cleaner.SetActiveState(state: true);
			}
		})
			.OnComplete(delegate
			{
				onCompleteAction?.Invoke();
				if (!state)
				{
					m_Cleaner.SetActiveState(state: false);
				}
			});
		RectTransform.DOAnchorPosY(state ? m_ShowPosY : m_HidePosY, m_SpeakerContentFadeTime).SetEase(state ? Ease.OutCubic : Ease.InCubic).SetUpdate(isIndependentUpdate: true);
	}

	protected virtual void CreateNavigation()
	{
		if (NavigationBehaviour == null)
		{
			AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
			CreateInput();
		}
		else
		{
			NavigationBehaviour.Clear();
		}
		NavigationBehaviour.SetEntitiesVertical(Answers);
		EventBus.RaiseEvent(delegate(IDialogNavigationCreatedHandler h)
		{
			h.HandleDialogNavigationCreated();
		});
	}

	protected virtual void CreateInput()
	{
		InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Dialog"
		});
		CreateInputImpl(InputLayer);
		AddDisposable(InputLayer.LayerBinded.Subscribe(OnLayerBindedChanged));
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	private void OnLayerBindedChanged(bool value)
	{
		foreach (TAnswerView answer in Answers)
		{
			answer.SetHasOverlay(!value);
		}
	}

	protected virtual void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_AnswerScrollRect.EnsureVisibleVertical(targetRect, 50f, smoothly: false, needPinch: false);
		}
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(OnConfirmClick, 8));
		AddDisposable(NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		(NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
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

	public void HandleCargoRewardsShow()
	{
		IsCargoRewardsOpen = true;
	}

	public void HandleCargoRewardsHide()
	{
		IsCargoRewardsOpen = false;
		NavigationBehaviour.FocusOnEntityManual(Answers.FirstItem());
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		m_AnswerScrollRect.ScrollToTop();
		NavigationBehaviour.FocusOnEntityManual(Answers.FirstItem());
	}
}
