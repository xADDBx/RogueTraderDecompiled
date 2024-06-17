using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Controllers.Dialog;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

[RequireComponent(typeof(DialogColorsConfig))]
public class DialogPCView : ViewBase<DialogVM>
{
	[Header("Answer Block")]
	[SerializeField]
	private TextMeshProUGUI m_AnswerName;

	[SerializeField]
	private Image m_AnswererPortrait;

	[Header("Speaker Block")]
	[SerializeField]
	private TextMeshProUGUI m_SpeakerName;

	[SerializeField]
	private Image m_SpeakerPortrait;

	[SerializeField]
	private Image m_SpeakerHolder;

	[Header("Body Block")]
	[SerializeField]
	private DialogCuePCView m_CueView;

	[SerializeField]
	private DialogAnswerPCView m_AnswerView;

	[SerializeField]
	private RectTransform m_AnswerPanel;

	[SerializeField]
	private DialogSystemAnswerPCView m_SystemAnswerPCView;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

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

	private bool m_VisibleState;

	private DialogColors m_DialogColors;

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : this.EnsureComponent<CanvasGroup>());

	private RectTransform RectTransform => m_RectTransform = (m_RectTransform ? m_RectTransform : GetComponent<RectTransform>());

	public void Initialize()
	{
		m_DialogColors = GetComponent<DialogColorsConfig>().DialogColors;
		m_SystemAnswerPCView.Initialize();
		m_CueView.Initialize(null, m_DialogColors);
		base.gameObject.SetActive(value: true);
		SetVisible(state: false, force: true);
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
			m_SpeakerPortrait.gameObject.SetActive(value != null);
			if (value != null)
			{
				m_SpeakerPortrait.sprite = value;
			}
		}));
		AddDisposable(base.ViewModel.EmptySpeaker.Subscribe(delegate(bool value)
		{
			m_SpeakerHolder.gameObject.SetActive(value);
		}));
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
			foreach (AnswerVM answer in answers)
			{
				DialogAnswerPCView widget = WidgetFactory.GetWidget(m_AnswerView, activate: true, strictMatching: true);
				widget.Initialize(m_DialogColors);
				widget.Bind(answer);
				widget.transform.SetParent(m_AnswerPanel, worldPositionStays: false);
			}
		}));
		AddDisposable(base.ViewModel.SystemAnswer.Subscribe(m_SystemAnswerPCView.Bind));
		AddDisposable(base.ViewModel.IsVisible.Subscribe(delegate(bool value)
		{
			SetVisible(value);
		}));
		AddDisposable(base.ViewModel.OnCueUpdate.Subscribe(StartUpdateCoroutine));
		StartUpdateCoroutine();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void StartUpdateCoroutine()
	{
		MainThreadDispatcher.StartUpdateMicroCoroutine(CurrentPartUpdateCoroutine());
	}

	protected override void DestroyViewImplementation()
	{
		SetVisible(state: false, force: false, delegate
		{
			m_HistoryEntities.ForEach(delegate(DialogHistoryEntity widget)
			{
				widget.Dispose();
				WidgetFactory.DisposeWidget(widget);
			});
			m_HistoryEntities.Clear();
		});
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
		}
		else
		{
			CanvasGroup.DOFade(state ? 1f : 0f, 0.2f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				onCompleteAction?.Invoke();
			});
			RectTransform.DOAnchorPosY(state ? m_ShowPosY : m_HidePosY, 0.2f).SetEase(state ? Ease.OutCubic : Ease.InCubic).SetUpdate(isIndependentUpdate: true);
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
