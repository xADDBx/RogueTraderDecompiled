using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.Pantograph;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;

public class JournalNavigationBaseView : ViewBase<JournalNavigationVM>, ISetCurrentQuestHandler, ISubscriber, IUpdateCanCompleteOrderNotificationHandler, ICanAccessContractsHandler
{
	[Header("Tabs")]
	[SerializeField]
	private OwlcatMultiButton m_QuestsButton;

	[SerializeField]
	private OwlcatMultiButton m_RumorsButton;

	[SerializeField]
	private OwlcatMultiButton m_OrdersButton;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[Header("Navigation Objects")]
	[SerializeField]
	[UsedImplicitly]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private GameObject m_CurrentQuest;

	[Header("Rumours")]
	[SerializeField]
	protected GameObject m_RumoursTitleGO;

	[SerializeField]
	protected GameObject m_RumoursAboutUsButtonGO;

	[Header("Localization")]
	[SerializeField]
	private TextMeshProUGUI m_RumoursTitleText;

	[SerializeField]
	private TextMeshProUGUI m_RumoursAboutUsTitleText;

	[SerializeField]
	private RectTransform m_ReadyToCompleteOrderImage;

	[SerializeField]
	private RectTransform m_CannotAccessContractsImage;

	[SerializeField]
	protected RectTransform m_EmptyListObject;

	[SerializeField]
	protected TextMeshProUGUI m_EmptyListLabel;

	private bool m_IsInit;

	private readonly string m_ActiveTabLayer = "Active";

	private readonly string m_UnactiveTabLayer = "Unactive";

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected bool ShowCompleted => Game.Instance.Player.UISettings.JournalShowCompletedQuest;

	protected WidgetListMVVM WidgetList => m_WidgetList;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableExtensions.Subscribe(m_QuestsButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetActiveTab(JournalTab.Quests);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_RumorsButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetActiveTab(JournalTab.Rumors);
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_OrdersButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetActiveTab(JournalTab.Orders);
		}));
		AddDisposable(base.ViewModel.ActiveTab.AsObservable().Subscribe(UpdateActiveTab));
		m_OrdersButton.SetInteractable(!base.ViewModel.CannotAccessContracts);
		AddDisposable(EventBus.Subscribe(this));
		BindPantograph();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void OnShowCompletedToggleChanged(bool value)
	{
		Game.Instance.Player.UISettings.JournalShowCompletedQuest = value;
		DrawEntities();
		switch (JournalHelper.CurrentQuest?.State)
		{
		case null:
		case QuestState.Completed:
		case QuestState.Failed:
		{
			Quest currentQuest = GetCurrentQuest(null);
			if (!JournalHelper.ChangeCurrentQuest(currentQuest))
			{
				base.ViewModel.SelectQuest(currentQuest);
			}
			break;
		}
		}
	}

	public virtual void DrawEntities()
	{
		m_WidgetList.Clear();
		m_RumoursTitleGO.SetActive(value: false);
		m_RumoursAboutUsButtonGO.SetActive(value: false);
		m_RumoursTitleText.text = UIStrings.Instance.QuesJournalTexts.AllRumoursTitle;
		m_RumoursAboutUsTitleText.text = UIStrings.Instance.QuesJournalTexts.RumoursAboutUsTitle;
		m_OrdersButton.SetInteractable(!base.ViewModel.CannotAccessContracts);
		m_CannotAccessContractsImage.Or(null)?.gameObject.SetActive(Game.Instance.Player.CannotAccessContracts.Value);
	}

	protected void ScrollToTop()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_ScrollRect.ScrollToTop();
			((RectTransform)m_ScrollRect.content.transform).anchoredPosition = Vector2.zero;
		}, 1);
	}

	protected void ScrollToRect()
	{
		RectTransform rectTransform = (Game.Instance.IsControllerMouse ? (GetCurrentEntityPC(null)?.transform as RectTransform) : (GetCurrentEntityConsole(null)?.transform as RectTransform));
		if (rectTransform != null && !m_ScrollRect.IsInViewport(rectTransform))
		{
			m_ScrollRect.ScrollToRectCenter(rectTransform, rectTransform);
		}
	}

	private JournalNavigationGroupElementPCView GetCurrentEntityPC(Quest currentQuest)
	{
		List<JournalNavigationGroupElementPCView> list = new List<JournalNavigationGroupElementPCView>();
		if (m_WidgetList.Entries != null)
		{
			foreach (IWidgetView entry in m_WidgetList.Entries)
			{
				if (entry is JournalNavigationGroupPCView journalNavigationGroupPCView)
				{
					list.AddRange(from JournalNavigationGroupElementPCView i in journalNavigationGroupPCView.WidgetList.Entries
						where i.IsActive
						select i);
				}
				else
				{
					list.AddRange(from JournalNavigationGroupElementPCView i in m_WidgetList.Entries
						where i.IsActive
						select i);
				}
				if (!list.Any())
				{
					list.AddRange((entry is JournalNavigationGroupPCView journalNavigationGroupPCView2) ? journalNavigationGroupPCView2.WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>() : m_WidgetList.Entries.Cast<JournalNavigationGroupElementPCView>());
				}
			}
		}
		if (currentQuest == null)
		{
			JournalNavigationGroupElementPCView journalNavigationGroupElementPCView = list.FirstOrDefault();
			if (journalNavigationGroupElementPCView != null)
			{
				return journalNavigationGroupElementPCView;
			}
		}
		return list.FirstOrDefault((JournalNavigationGroupElementPCView elementView) => elementView.Quest == currentQuest);
	}

	private JournalNavigationGroupElementConsoleView GetCurrentEntityConsole(Quest currentQuest)
	{
		List<JournalNavigationGroupElementConsoleView> list = new List<JournalNavigationGroupElementConsoleView>();
		if (m_WidgetList.Entries != null)
		{
			foreach (IWidgetView entry in m_WidgetList.Entries)
			{
				if (entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView)
				{
					list.AddRange(from JournalNavigationGroupElementConsoleView i in journalNavigationGroupConsoleView.WidgetList.Entries
						where i.IsActive
						select i);
				}
				else
				{
					list.AddRange(from JournalNavigationGroupElementConsoleView i in m_WidgetList.Entries
						where i.IsActive
						select i);
				}
				if (!list.Any())
				{
					list.AddRange((entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView2) ? journalNavigationGroupConsoleView2.WidgetList.Entries.Cast<JournalNavigationGroupElementConsoleView>() : m_WidgetList.Entries.Cast<JournalNavigationGroupElementConsoleView>());
				}
			}
		}
		if (currentQuest == null)
		{
			JournalNavigationGroupElementConsoleView journalNavigationGroupElementConsoleView = list.FirstOrDefault();
			if (journalNavigationGroupElementConsoleView != null)
			{
				return journalNavigationGroupElementConsoleView;
			}
		}
		return list.FirstOrDefault((JournalNavigationGroupElementConsoleView elementView) => elementView.Quest == currentQuest);
	}

	public Quest GetCurrentQuest(Quest currentQuest)
	{
		if (!Game.Instance.IsControllerMouse)
		{
			return GetCurrentEntityConsole(currentQuest)?.Quest;
		}
		return GetCurrentEntityPC(currentQuest)?.Quest;
	}

	private void UpdateActiveTab(JournalTab activeTab)
	{
		m_QuestsButton.SetActiveLayer((activeTab == JournalTab.Quests) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_RumorsButton.SetActiveLayer((activeTab == JournalTab.Rumors) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_OrdersButton.SetActiveLayer((activeTab == JournalTab.Orders) ? m_ActiveTabLayer : m_UnactiveTabLayer);
		m_EmptyListLabel.text = string.Format(UIStrings.Instance.QuesJournalTexts.NoNameOfTheListObjectsAvailable, UIStrings.Instance.QuesJournalTexts.GetActiveTabLabel(activeTab));
		m_ReadyToCompleteOrderImage.Or(null)?.gameObject.SetActive(base.ViewModel.CheckReadyToCompleteOrders() && !Game.Instance.Player.CannotAccessContracts.Value);
		DrawEntities();
		ScrollToTop();
		Quest currentQuest = GetCurrentQuest(null);
		m_CurrentQuest.SetActive(currentQuest != null);
		base.ViewModel.SelectQuest(currentQuest);
	}

	public void OnPrevActiveTab()
	{
		base.ViewModel.OnPrevActiveTab();
	}

	public void OnNextActiveTab()
	{
		base.ViewModel.OnNextActiveTab();
	}

	void ISetCurrentQuestHandler.HandleSetCurrentQuest(Quest quest)
	{
		m_CurrentQuest.SetActive(quest != null);
	}

	public void HandleElementSelected(IQuestEntity element)
	{
		base.ViewModel.SelectQuest(element.Quest);
	}

	private void BindPantograph()
	{
		if (Game.Instance.IsControllerMouse)
		{
			JournalNavigationGroupElementPCView entity = GetCurrentEntityPC(null);
			if ((bool)entity && entity.gameObject.activeInHierarchy)
			{
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Bind(entity.PantographConfig);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Unbind();
				});
			}
		}
		else
		{
			if (!Game.Instance.IsControllerGamepad)
			{
				return;
			}
			JournalNavigationGroupElementConsoleView entity = GetCurrentEntityConsole(null);
			if ((bool)entity && entity.gameObject.activeInHierarchy)
			{
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Bind(entity.PantographConfig);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(IPantographHandler h)
				{
					h.Unbind();
				});
			}
		}
	}

	public JournalTab GetActiveTab()
	{
		return base.ViewModel.ActiveTab.Value;
	}

	public void HandleUpdateCanCompleteOrderNotificationInJournal()
	{
		m_ReadyToCompleteOrderImage.Or(null)?.gameObject.SetActive(base.ViewModel.CheckReadyToCompleteOrders() && !Game.Instance.Player.CannotAccessContracts.Value);
	}

	public void HandleCanAccessContractsChanged()
	{
		m_OrdersButton.SetInteractable(!Game.Instance.Player.CannotAccessContracts.Value);
		m_CannotAccessContractsImage.Or(null)?.gameObject.SetActive(Game.Instance.Player.CannotAccessContracts.Value);
		m_ReadyToCompleteOrderImage.Or(null)?.gameObject.SetActive(base.ViewModel.CheckReadyToCompleteOrders() && !Game.Instance.Player.CannotAccessContracts.Value);
	}
}
