using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalConsoleView : JournalBaseView
{
	[Header("Console")]
	[SerializeField]
	private JournalNavigationConsoleView m_NavigationView;

	[SerializeField]
	private JournalQuestConsoleView m_QuestView;

	[SerializeField]
	private JournalRumourConsoleView m_RumourView;

	[SerializeField]
	private JournalOrderConsoleView m_OrderView;

	[Header("Hints")]
	[SerializeField]
	[UsedImplicitly]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly BoolReactiveProperty m_CanExpand = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanCollapse = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsShowCompletedQuests = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsQuestEntity = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsConfirmOrder = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasFinishedQuests = new BoolReactiveProperty();

	public override void Initialize()
	{
		m_NavigationView.Initialize();
		m_QuestView.Initialize();
		m_RumourView.Initialize();
		m_OrderView.Initialize();
		base.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationView.Bind(base.ViewModel.Navigation);
		m_SelectorView.Bind(base.ViewModel.Selector);
		AddDisposable(base.ViewModel.UpdateView.Subscribe(OnSelectedQuestChange));
		OnSelectedQuestChange(base.ViewModel.SelectedQuest.Value);
		CreateInput();
		UpdateNavigation();
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(m_NavigationView.ScrollMenu));
		m_IsShowCompletedQuests.Value = Game.Instance.Player.UISettings.JournalShowCompletedQuest;
	}

	private void UpdateNavigation()
	{
		m_InputLayer.Unbind();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesVertical(m_NavigationView.GetNavigationEntities());
		m_InputLayer.Bind();
		CheckHasFinishedQuests();
		foreach (IConsoleEntity entity in m_NavigationBehaviour.Entities)
		{
			if (entity is JournalNavigationGroupElementConsoleView journalNavigationGroupElementConsoleView && entity.IsValid() && journalNavigationGroupElementConsoleView.IsSelected)
			{
				m_NavigationBehaviour.FocusOnEntityManual(entity);
				m_NavigationView.HandleElementSelected(journalNavigationGroupElementConsoleView);
				break;
			}
		}
	}

	private void CheckHasFinishedQuests()
	{
		if (base.ViewModel.Navigation.ActiveTab.Value == JournalTab.Quests)
		{
			m_HasFinishedQuests.Value = base.ViewModel.Navigation.NavigationGroups.Any((JournalNavigationGroupVM ng) => ng.Quests.Any((JournalQuestVM q) => !q.IsActive));
		}
		if (base.ViewModel.Navigation.ActiveTab.Value == JournalTab.Rumors)
		{
			m_HasFinishedQuests.Value = base.ViewModel.Navigation.Rumors.Any((JournalQuestVM r) => r.Quest.Blueprint.Type == QuestType.Rumour && !r.IsActive) || base.ViewModel.Navigation.Rumors.Any((JournalQuestVM r) => r.Quest.Blueprint.Type == QuestType.RumourAboutUs && !r.IsActive);
		}
		if (base.ViewModel.Navigation.ActiveTab.Value == JournalTab.Orders)
		{
			m_HasFinishedQuests.Value = base.ViewModel.Navigation.Orders.Any((JournalQuestVM o) => !o.IsActive);
		}
	}

	private void OnSelectedQuestChange(Quest selectedQuest)
	{
		m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
		if (selectedQuest != null && selectedQuest.Blueprint.Group == QuestGroupId.Rumours)
		{
			TryBindQuestView(base.ViewModel.Navigation.Rumors);
		}
		if (selectedQuest != null && selectedQuest.Blueprint.Group == QuestGroupId.Orders)
		{
			TryBindQuestView(base.ViewModel.Navigation.Orders);
			return;
		}
		foreach (JournalNavigationGroupVM navigationGroup in base.ViewModel.Navigation.NavigationGroups)
		{
			if (TryBindQuestView(navigationGroup.Quests))
			{
				break;
			}
		}
	}

	private bool TryBindQuestView(IEnumerable<JournalQuestVM> quests)
	{
		foreach (JournalQuestVM quest in quests)
		{
			if (quest.IsSelected.Value)
			{
				BaseJournalItemConsoleView baseJournalItemConsoleView = (quest.IsRumour ? m_RumourView : (quest.IsOrder ? ((BaseJournalItemConsoleView)m_OrderView) : ((BaseJournalItemConsoleView)m_QuestView)));
				BaseJournalItemConsoleView baseJournalItemConsoleView2 = (quest.IsRumour ? m_QuestView : (quest.IsOrder ? ((BaseJournalItemConsoleView)m_RumourView) : ((BaseJournalItemConsoleView)m_OrderView)));
				BaseJournalItemConsoleView obj = (quest.IsRumour ? m_OrderView : (quest.IsOrder ? ((BaseJournalItemConsoleView)m_QuestView) : ((BaseJournalItemConsoleView)m_RumourView)));
				baseJournalItemConsoleView.Bind(quest);
				baseJournalItemConsoleView2.Unbind();
				obj.Unbind();
				return true;
			}
		}
		return false;
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Journal"
		});
		AddDisposable(m_NavigationBehaviour.Focus.Subscribe(delegate
		{
			m_IsQuestEntity.Value = m_NavigationBehaviour.CurrentEntity is JournalNavigationGroupElementConsoleView;
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}));
		AddDisposable(m_InputLayer.AddAxis(Scroll, 3, repeat: true));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowCompletedQuestsChange();
		}, 11, m_IsShowCompletedQuests.And(m_HasFinishedQuests).ToReactiveProperty()), UIStrings.Instance.QuesJournalTexts.HideCompletedQuests));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowCompletedQuestsChange();
		}, 11, m_IsShowCompletedQuests.Not().And(m_HasFinishedQuests).ToReactiveProperty()), UIStrings.Instance.QuesJournalTexts.ShowCompletedQuests));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			CloseWindow();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_PrevHint.Bind(m_InputLayer.AddButton(delegate
		{
			m_NavigationView.OnPrevActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
			UpdateNavigation();
		}, 14)));
		AddDisposable(m_NextHint.Bind(m_InputLayer.AddButton(delegate
		{
			m_NavigationView.OnNextActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
			UpdateNavigation();
		}, 15)));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Collapse();
		}, 8, m_CanCollapse), UIStrings.Instance.CommonTexts.Collapse));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Expand();
		}, 8, m_CanExpand), UIStrings.Instance.CommonTexts.Expand));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnFocusedChanged(m_NavigationBehaviour.CurrentEntity);
		}, 8, m_IsQuestEntity), UIStrings.Instance.CommonTexts.Select));
		AddDisposable(m_OrderView.CompleteOrderHint.Bind(m_InputLayer.AddButton(delegate
		{
			m_OrderView.CompleteOrder();
			m_IsConfirmOrder.Value = false;
		}, 10, m_IsConfirmOrder)));
		m_OrderView.CompleteOrderHint.SetLabel(UIStrings.Instance.QuesJournalTexts.CompleteOrder);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		AddDisposable(m_CanvasSortingComponent.PushView());
	}

	private bool IsConfirmOrder()
	{
		if (m_OrderView.gameObject.activeSelf && m_OrderView.CompleteOrderButton.gameObject.activeSelf && m_OrderView.CompleteOrderButton.Interactable)
		{
			return m_OrderView.IsOrderCompleted();
		}
		return false;
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		if (m_NavigationView.GetActiveTab() == JournalTab.Quests)
		{
			m_QuestView.Scroll(obj, value);
		}
		if (m_NavigationView.GetActiveTab() == JournalTab.Rumors)
		{
			m_RumourView.Scroll(obj, value);
		}
		else
		{
			m_OrderView.Scroll(obj, value);
		}
	}

	private void Expand()
	{
		OwlcatMultiButton owlcatMultiButton = m_NavigationBehaviour.Focus.Value as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>().Expand();
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}
	}

	private void Collapse()
	{
		OwlcatMultiButton owlcatMultiButton = m_NavigationBehaviour.Focus.Value as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>().Collapse();
			UpdateExpandableElementFlags(m_NavigationBehaviour.CurrentEntity);
		}
	}

	private void ShowCompletedQuestsChange()
	{
		bool journalShowCompletedQuest = Game.Instance.Player.UISettings.JournalShowCompletedQuest;
		m_NavigationView.OnShowCompletedToggleChanged(!journalShowCompletedQuest);
		m_IsShowCompletedQuests.Value = !journalShowCompletedQuest;
		UpdateNavigation();
	}

	public void UpdateExpandableElementFlags(IConsoleEntity entity)
	{
		if (entity is JournalNavigationGroupElementConsoleView)
		{
			m_CanExpand.Value = false;
			m_CanCollapse.Value = false;
		}
		OwlcatMultiButton owlcatMultiButton = entity as OwlcatMultiButton;
		if (!(owlcatMultiButton == null) && (bool)owlcatMultiButton.gameObject.GetComponent<ExpandableCollapseMultiButtonConsole>())
		{
			ExpandableCollapseMultiButtonConsole component = owlcatMultiButton.GetComponent<ExpandableCollapseMultiButtonConsole>();
			m_CanExpand.Value = !component.IsOn.Value;
			m_CanCollapse.Value = component.IsOn.Value;
		}
	}

	public void OnFocusedChanged(IConsoleEntity entity)
	{
		if (entity is JournalNavigationGroupElementConsoleView element)
		{
			m_NavigationView.HandleElementSelected(element);
			m_IsConfirmOrder.Value = IsConfirmOrder();
		}
	}

	private void CloseWindow()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}
}
