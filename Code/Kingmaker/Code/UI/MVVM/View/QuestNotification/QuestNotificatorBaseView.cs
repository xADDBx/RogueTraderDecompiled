using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Enums;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.GameConst;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.QuestNotification;

public class QuestNotificatorBaseView : ViewBase<QuestNotificatorVM>
{
	private readonly Queue<Action> m_QuestNotificationsQueue = new Queue<Action>();

	private readonly Queue<Action> m_ObjectiveNotificationsQueue = new Queue<Action>();

	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private QuestNotificationQuestView m_QuestView;

	private QuestNotificationQuestVM m_CurrentQuest;

	[SerializeField]
	private QuestNotificationObjectivesView m_ObjectiveView;

	[SerializeField]
	private QuestNotificationAddendumView m_AddendumView;

	private QuestNotificationEntityVM m_CurrentObjective;

	[SerializeField]
	private WindowAnimator m_Animator;

	private bool HasQuestNotifications => m_QuestNotificationsQueue.Any();

	private bool HasObjectiveNotifications => m_ObjectiveNotificationsQueue.Any();

	public void Initialize()
	{
		m_Animator.Initialize();
		m_QuestView.Initialize();
		m_ObjectiveView.Initialize();
		m_AddendumView.Initialize();
		Hide();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.QuestEntities.ObserveAdd().Subscribe(delegate(CollectionAddEvent<QuestNotificationQuestVM> q)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				AddQuestShowEvent(q.Value);
			}, 1);
		}));
		AddDisposable(base.ViewModel.ObjectiveEntities.ObserveAdd().Subscribe(delegate(CollectionAddEvent<QuestNotificationEntityVM> q)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				AddObjectiveShowEvent(q.Value);
			}, 1);
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ForceCloseCommand, delegate
		{
			CheckJournalButtons();
		}));
		AddDisposable(base.ViewModel.IsShowUp.Subscribe(delegate(bool value)
		{
			if (!value)
			{
				base.gameObject.SetActive(value: false);
			}
			else
			{
				UISounds.Instance.Sounds.Journal.NewQuest.Play();
			}
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ClearCommand, delegate
		{
			m_QuestNotificationsQueue.Clear();
			m_ObjectiveNotificationsQueue.Clear();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	protected void OpenJournal()
	{
		if (Game.Instance.CurrentMode != GameModeType.GameOver)
		{
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleOpenJournal();
			});
		}
		Hide();
	}

	protected void Close()
	{
		Hide();
	}

	protected virtual void CheckJournalButtons()
	{
	}

	protected bool CheckActiveToJournalButtons()
	{
		bool num = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag = Game.Instance.CurrentMode == GameModeType.Cutscene;
		bool flag2 = Game.Instance.CurrentMode == GameModeType.Dialog;
		if (!num && !flag)
		{
			return !flag2;
		}
		return false;
	}

	private void AddQuestShowEvent(QuestNotificationQuestVM quest)
	{
		m_QuestNotificationsQueue.Enqueue(delegate
		{
			QuestNotificationsAction(quest);
		});
		Tick();
	}

	private void QuestNotificationsAction(QuestNotificationQuestVM quest)
	{
		quest.SetCurrentQuest();
		m_CurrentQuest = quest;
		base.gameObject.SetActive(quest.Quest != null);
		m_Title.gameObject.SetActive(value: true);
		m_Title.text = quest.Title;
		m_QuestView.Bind(quest);
		UISounds.Instance.Sounds.Journal.NewQuest.Play();
	}

	private void AddObjectiveShowEvent(QuestNotificationEntityVM objective)
	{
		if (!string.IsNullOrWhiteSpace((!objective.IsAddendum) ? objective.Title : objective.Description) && !objective.IsErrandObjective && objective.Quest.Blueprint.Group != QuestGroupId.Rumours)
		{
			QuestType type = objective.Quest.Blueprint.Type;
			if (type != QuestType.Rumour && type != QuestType.RumourAboutUs)
			{
				MainThreadDispatcher.StartCoroutine(WaitForNotificationClose(objective));
			}
		}
	}

	private IEnumerator WaitForNotificationClose(QuestNotificationEntityVM objective)
	{
		while (base.ViewModel.IsShowUp.Value && (m_CurrentObjective != null || m_CurrentQuest.State == QuestNotificationState.New))
		{
			yield return null;
		}
		m_ObjectiveNotificationsQueue.Enqueue(delegate
		{
			ObjectiveNotificationsAction(objective);
		});
		Tick();
	}

	private void ObjectiveNotificationsAction(QuestNotificationEntityVM objective)
	{
		objective.SetCurrentQuest();
		m_CurrentObjective = objective;
		m_Title.gameObject.SetActive(value: true);
		m_Title.text = objective.QuestName;
		m_AddendumView.Bind(objective.IsAddendum ? objective : null);
		m_ObjectiveView.Bind(objective.IsAddendum ? null : objective);
	}

	public void Tick()
	{
		if (!base.ViewModel.ForbiddenQuestNotification && (HasQuestNotifications || HasObjectiveNotifications))
		{
			ShowNextNotification();
		}
		if (HasQuestNotifications || HasObjectiveNotifications)
		{
			MainThreadDispatcher.Post(delegate
			{
				Tick();
			}, null);
		}
	}

	protected virtual void ShowNextNotification()
	{
		CheckJournalButtons();
		base.ViewModel.IsShowUp.Value = true;
		m_Animator.AppearAnimation(delegate
		{
			StartCoroutine(HideCurrentNotification());
		});
		if (HasQuestNotifications)
		{
			m_QuestNotificationsQueue.Dequeue()();
		}
		else if (HasObjectiveNotifications)
		{
			m_ObjectiveNotificationsQueue.Dequeue()();
		}
	}

	private IEnumerator HideCurrentNotification()
	{
		yield return new WaitForSecondsRealtime(UIConsts.QuestNotificationTime / (float)((!Game.Instance.TurnController.TurnBasedModeActive) ? 1 : 2));
		Hide();
	}

	protected virtual void Hide()
	{
		m_Animator.DisappearAnimation(delegate
		{
			m_Title.gameObject.SetActive(value: false);
			if (m_CurrentQuest != null)
			{
				base.ViewModel.HandleQuestShowed(m_CurrentQuest);
				m_CurrentQuest = null;
			}
			if (m_CurrentObjective != null)
			{
				base.ViewModel.HandleObjectiveShowed(m_CurrentObjective);
				m_CurrentObjective = null;
			}
			if (!HasObjectiveNotifications || !HasQuestNotifications)
			{
				base.ViewModel.IsShowUp.Value = false;
			}
		});
	}
}
