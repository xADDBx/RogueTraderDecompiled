using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

[RequireComponent(typeof(DialogColorsConfig))]
public class SpaceEventBaseView<DialogAnswer, DialogSystem> : ViewBase<SpaceEventVM>, IGameModeHandler, ISubscriber where DialogAnswer : DialogAnswerBaseView where DialogSystem : DialogSystemAnswerBaseView
{
	[Header("Body Block")]
	[SerializeField]
	protected DialogCuePCView m_CueView;

	[SerializeField]
	protected DialogAnswer m_AnswerView;

	[SerializeField]
	private RectTransform m_AnswerPanel;

	[SerializeField]
	protected DialogSystem m_SystemAnswerView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private VerticalLayoutGroup m_CurrentPartLayoutGroup;

	[Header("History Block")]
	[SerializeField]
	private DialogHistoryEntity m_HistoryEntity;

	[SerializeField]
	private RectTransform m_HistoryContainer;

	[Header("Notification Block")]
	[SerializeField]
	private DialogNotificationsPCView m_DialogNotifications;

	[Header("Common Block")]
	[SerializeField]
	private int m_HidePosY = -358;

	[SerializeField]
	private int m_ShowPosY;

	private readonly List<DialogHistoryEntity> m_HistoryEntities = new List<DialogHistoryEntity>();

	private CanvasGroup m_CanvasGroup;

	private RectTransform m_RectTransform;

	private DialogColors m_DialogColors;

	private bool m_VisibleState;

	protected GridConsoleNavigationBehaviour NavigationBehaviour;

	protected List<DialogAnswer> m_Answers = new List<DialogAnswer>();

	protected InputLayer InputLayer;

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : this.EnsureComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		m_DialogColors = GetComponent<DialogColorsConfig>().DialogColors;
		m_SystemAnswerView.Initialize();
		m_CueView.Initialize(null, m_DialogColors);
		SetVisible(state: false, force: true);
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_DialogNotifications.Bind(base.ViewModel.DialogNotifications);
		AddDisposable(base.ViewModel.Cue.Subscribe(m_CueView.Bind));
		AddDisposable(base.ViewModel.History.ObserveAdd().Subscribe(delegate(CollectionAddEvent<IDialogShowData> value)
		{
			DialogHistoryEntity widget2 = WidgetFactory.GetWidget(m_HistoryEntity, activate: true, strictMatching: true);
			widget2.Initialize(value.Value.GetText(m_DialogColors));
			widget2.transform.SetParent(m_HistoryContainer, worldPositionStays: false);
			m_HistoryEntities.Add(widget2);
		}));
		AddDisposable(base.ViewModel.Answers.Subscribe(delegate(List<AnswerVM> answers)
		{
			if (answers == null)
			{
				return;
			}
			ClearAnswers();
			foreach (AnswerVM answer in answers)
			{
				DialogAnswer widget = WidgetFactory.GetWidget(m_AnswerView, activate: true, strictMatching: true);
				widget.Initialize(m_DialogColors);
				widget.Bind(answer);
				widget.transform.SetParent(m_AnswerPanel, worldPositionStays: false);
				m_Answers.Add(widget);
			}
		}));
		AddDisposable(base.ViewModel.SystemAnswer.Subscribe(BindSystemAnswer));
		AddDisposable(base.ViewModel.OnCueUpdate.Subscribe(delegate
		{
			MainThreadDispatcher.StartUpdateMicroCoroutine(CurrentPartUpdateCoroutine());
		}));
		AddDisposable(EventBus.Subscribe(this));
		SetVisible(state: true);
	}

	private void BindSystemAnswer(AnswerVM answerVM)
	{
		if (answerVM != null)
		{
			m_SystemAnswerView.Bind(answerVM);
			ClearAnswers();
			if (NavigationBehaviour == null)
			{
				AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
				CreateInput();
			}
			NavigationBehaviour.AddEntityVertical(m_SystemAnswerView);
			NavigationBehaviour.FocusOnEntityManual(m_SystemAnswerView);
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		SetVisible(state: false, force: false, delegate
		{
			m_HistoryEntities.ForEach(WidgetFactory.DisposeWidget);
			m_HistoryEntities.Clear();
		});
		NavigationBehaviour?.Dispose();
		NavigationBehaviour = null;
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
		NavigationBehaviour.SetEntitiesVertical(m_Answers);
		NavigationBehaviour.AddEntityVertical(m_SystemAnswerView);
		NavigationBehaviour.FocusOnFirstValidEntity();
	}

	protected virtual void CreateInput()
	{
		InputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Dialog"
		});
		CreateInputImpl(InputLayer);
		AddDisposable(GamePad.Instance.PushLayer(InputLayer));
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(OnConfirmClick, 8));
		AddDisposable(inputLayer.AddAxis(Scroll, 3));
		AddDisposable(NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged));
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_ScrollRect.Scroll(value, smooth: true);
	}

	private void OnConfirmClick(InputActionEventData obj)
	{
		(NavigationBehaviour.CurrentEntity as IConfirmClickHandler)?.OnConfirmClick();
	}

	protected virtual void OnFocusChanged(IConsoleEntity focus)
	{
		if (focus != null)
		{
			RectTransform targetRect = ((focus as MonoBehaviour) ?? (focus as IMonoBehaviour)?.MonoBehaviour)?.transform as RectTransform;
			m_ScrollRect.EnsureVisibleVertical(targetRect);
		}
	}

	private void CalculateHeight()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_ScrollRect.viewport.transform as RectTransform);
		float num = m_CurrentPartLayoutGroup.padding.top;
		for (int i = 0; i < m_CurrentPartLayoutGroup.transform.childCount; i++)
		{
			Transform child = m_CurrentPartLayoutGroup.transform.GetChild(i);
			if (child.gameObject.activeInHierarchy)
			{
				num += LayoutUtility.GetPreferredHeight(child as RectTransform) + m_CurrentPartLayoutGroup.spacing;
			}
		}
		num += LayoutUtility.GetPreferredHeight(m_CueView.transform as RectTransform);
		num += LayoutUtility.GetPreferredHeight(m_DialogNotifications.transform as RectTransform);
		float height = m_ScrollRect.viewport.rect.height;
		m_CurrentPartLayoutGroup.padding.bottom = Mathf.FloorToInt(height - Mathf.Clamp(num, 0f, height));
	}

	private IEnumerator CurrentPartUpdateCoroutine()
	{
		m_ScrollRect.content.EnsureComponent<CanvasGroup>().alpha = 0f;
		yield return null;
		CalculateHeight();
		yield return null;
		m_ScrollRect.ScrollToBottom();
		yield return null;
		m_ScrollRect.EnsureVisibleVertical(m_CueView.transform as RectTransform, 0f, smoothly: false, needPinch: false);
		yield return null;
		m_ScrollRect.content.EnsureComponent<CanvasGroup>().alpha = 1f;
		yield return null;
		m_CueView.m_CueGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		CreateNavigation();
	}

	private void ClearAnswers()
	{
		m_Answers.ForEach(delegate(DialogAnswer a)
		{
			WidgetFactory.DisposeWidget(a);
		});
		m_Answers.Clear();
	}

	private void SetVisible(bool state, bool force = false, Action onCompleteAction = null)
	{
		if (m_VisibleState == state)
		{
			return;
		}
		m_VisibleState = state;
		if (force)
		{
			CanvasGroup.alpha = (state ? 1f : 0f);
			Vector2 anchoredPosition = RectTransform.anchoredPosition;
			anchoredPosition.y = (state ? m_ShowPosY : m_HidePosY);
			RectTransform.anchoredPosition = anchoredPosition;
			onCompleteAction?.Invoke();
			return;
		}
		CanvasGroup.DOFade(state ? 1f : 0f, 0.2f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			onCompleteAction?.Invoke();
		});
		RectTransform.DOAnchorPosY(state ? m_ShowPosY : m_HidePosY, 0.2f).SetEase(state ? Ease.OutCubic : Ease.InCubic).SetUpdate(isIndependentUpdate: true);
		if (state)
		{
			UISounds.Instance.Sounds.SpaceExploration.SystemEvent.Play();
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			SetVisible(state: false);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			SetVisible(state: true);
		}
	}
}
