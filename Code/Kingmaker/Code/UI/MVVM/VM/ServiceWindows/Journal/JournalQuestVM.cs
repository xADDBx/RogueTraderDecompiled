using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalQuestVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsOrderCompleted = new ReactiveProperty<bool>();

	public readonly AutoDisposingList<ColonyProjectsRewardElementVM> Rewards = new AutoDisposingList<ColonyProjectsRewardElementVM>();

	public readonly AutoDisposingList<ColonyProjectsRequirementElementVM> Requirements = new AutoDisposingList<ColonyProjectsRequirementElementVM>();

	public readonly Quest Quest;

	public readonly List<JournalQuestObjectiveVM> Objectives;

	public readonly string ServiceMessage;

	public readonly string Description;

	public readonly string Title;

	public readonly string CompletionText;

	public readonly string Place;

	public readonly Sprite DestinationImage;

	public bool IsNew;

	public bool IsCompleted;

	public bool IsUpdated;

	public bool IsPostponed;

	public bool IsFailed;

	public readonly bool IsLastChapter;

	public readonly bool IsAffectedByNomos;

	public bool CanCompleteOrder;

	public readonly bool IsAtDestinationSystem;

	public readonly bool HasDestinationImage;

	private readonly Action<Quest> m_SelectQuestCallback;

	public readonly AutoDisposingList<ColonyResourceVM> ResourcesVMs = new AutoDisposingList<ColonyResourceVM>();

	public readonly JournalOrderProfitFactorVM JournalOrderProfitFactorVM;

	public readonly ReactiveCommand RefreshData = new ReactiveCommand();

	public readonly float FontMultiplier = FontSizeMultiplier;

	public bool IsAttention => Objectives.Any((JournalQuestObjectiveVM ob) => ob.IsAttention);

	public bool IsActive
	{
		get
		{
			if (!IsFailed)
			{
				return !IsCompleted;
			}
			return false;
		}
	}

	public bool IsRumour => Quest.Blueprint.Group == QuestGroupId.Rumours;

	public bool IsOrder => Quest.Blueprint.Group == QuestGroupId.Orders;

	public bool IsViewed
	{
		get
		{
			if (Quest.IsViewed)
			{
				return ActiveObjectives.All((QuestObjective x) => x.IsViewed);
			}
			return false;
		}
	}

	public bool QuestIsViewed => Quest.IsViewed;

	public List<BlueprintResourceReference> BasicResources => BlueprintWarhammerRoot.Instance.ColonyRoot.BasicResources;

	private static List<BlueprintResourceReference> AllResources => BlueprintWarhammerRoot.Instance.ColonyRoot.AllResources;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	private IEnumerable<QuestObjective> ActiveObjectives => Quest.Objectives.Where((QuestObjective x) => x.IsActive && !x.Blueprint.IsHidden);

	public JournalQuestVM(Quest quest, ReactiveProperty<Quest> selectedQuest = null, Action<Quest> selectQuestCallback = null)
	{
		AddDisposable(JournalOrderProfitFactorVM = new JournalOrderProfitFactorVM());
		Quest = quest;
		Place = Quest.Blueprint.Place;
		Title = Quest.Blueprint.Title;
		Description = Quest.Blueprint.Description;
		CompletionText = ((quest.State == QuestState.Completed) ? ((string)quest.Blueprint.CompletionText) : string.Empty);
		ServiceMessage = Quest.Blueprint.ServiceMessage;
		UpdateStatus(quest);
		IsLastChapter = quest.Blueprint.LastChapter == Game.Instance.Player.Chapter && IsActive;
		IsAffectedByNomos = false;
		foreach (Requirement component2 in Quest.Blueprint.GetComponents<Requirement>())
		{
			ColonyProjectsRequirementElementVM item = new ColonyProjectsRequirementElementVM(component2, null, isJournal: true);
			Requirements.Add(item);
		}
		UpdateCanCompleteState();
		foreach (Reward component3 in Quest.Blueprint.GetComponents<Reward>())
		{
			ColonyProjectsRewardElementVM item2 = new ColonyProjectsRewardElementVM(component3);
			Rewards.Add(item2);
		}
		ReactiveProperty<bool> isOrderCompleted = IsOrderCompleted;
		QuestState state = quest.State;
		isOrderCompleted.Value = state == QuestState.Completed || state == QuestState.Failed;
		m_SelectQuestCallback = selectQuestCallback;
		if (selectedQuest != null)
		{
			AddDisposable(selectedQuest.Subscribe(OnSelectedQuestChanged));
		}
		List<QuestObjective> list = Quest.Objectives.Where((QuestObjective o) => o.IsVisible && o.State != 0 && !o.Blueprint.IsAddendum && !o.Blueprint.IsErrandObjective && !o.Blueprint.IsHidden).ToList();
		list.Sort(Comparison);
		SectorMapObjectEntity currentStarSystem = Game.Instance.SectorMapController.CurrentStarSystem;
		if (IsRumour && currentStarSystem != null)
		{
			foreach (QuestObjective item3 in list.Where((QuestObjective o) => o.State == QuestObjectiveState.Started))
			{
				RumourMapMarker component = item3.Blueprint.GetComponent<RumourMapMarker>();
				HasDestinationImage = component?.SectorMapDestinationImage != null;
				if (HasDestinationImage)
				{
					DestinationImage = component?.SectorMapDestinationImage;
				}
				if (component != null && component.SectorMapPointsToVisit.Dereference().Contains(currentStarSystem.Blueprint))
				{
					IsAtDestinationSystem = true;
					break;
				}
			}
		}
		Objectives = new List<JournalQuestObjectiveVM>();
		if (Objectives == null)
		{
			return;
		}
		foreach (QuestObjective item4 in list.Where((QuestObjective o) => !o.Blueprint.IsHidden))
		{
			Objectives?.Add(new JournalQuestObjectiveVM(item4));
		}
		UpdateData();
	}

	protected override void DisposeImplementation()
	{
		Rewards.Clear();
		Requirements.Clear();
		ResourcesVMs.Clear();
		Objectives.ForEach(delegate(JournalQuestObjectiveVM obj)
		{
			obj.Dispose();
		});
		Objectives.Clear();
	}

	private void Clear()
	{
		ResourcesVMs.Clear();
	}

	private void UpdateData()
	{
		Clear();
		AddResources(Game.Instance.ColonizationController.AllResourcesInPool());
	}

	private void AddResources(Dictionary<BlueprintResource, int> resources)
	{
		foreach (BlueprintResourceReference allResource in AllResources)
		{
			GetOrCreateResource(allResource);
		}
		foreach (KeyValuePair<BlueprintResource, int> resource in resources)
		{
			GetOrCreateResource(resource.Key).UpdateCount(resource.Value);
		}
		JournalOrderProfitFactorVM.UpdateCount(Game.Instance.Player.ProfitFactor.Total);
		if (Quest.State != QuestState.Completed)
		{
			ResourcesVMs.ForEach(delegate(ColonyResourceVM v)
			{
				v.UpdateArrowDirection(CheckArrowDirectionResources(v.BlueprintResource.Value));
			});
			JournalOrderProfitFactorVM.UpdateArrowDirection(CheckArrowDirectionProfitFactor());
		}
		RefreshData.Execute();
	}

	private int CheckArrowDirectionResources(BlueprintResource blueprintResource)
	{
		foreach (Requirement component in Quest.Blueprint.GetComponents<Requirement>())
		{
			if (component is RequirementResourceUseOrder requirementResourceUseOrder && requirementResourceUseOrder.ResourceBlueprint == blueprintResource)
			{
				return -1;
			}
		}
		foreach (Reward component2 in Quest.Blueprint.GetComponents<Reward>())
		{
			if (component2 is RewardResourceProject rewardResourceProject && rewardResourceProject.Resource == blueprintResource)
			{
				return 1;
			}
			if (component2 is RewardResourceNotFromColony rewardResourceNotFromColony && rewardResourceNotFromColony.Resource == blueprintResource)
			{
				return 1;
			}
		}
		return 0;
	}

	private int CheckArrowDirectionProfitFactor()
	{
		if (Requirements.OfType<RequirementProfitFactorCost>().Any())
		{
			return -1;
		}
		if (!Rewards.OfType<RewardProfitFactor>().Any())
		{
			return 0;
		}
		return 1;
	}

	private ColonyResourceVM GetOrCreateResource(BlueprintResource blueprintResource)
	{
		foreach (ColonyResourceVM resourcesVM in ResourcesVMs)
		{
			if (resourcesVM.BlueprintResource.Value == blueprintResource)
			{
				return resourcesVM;
			}
		}
		ColonyResourceVM colonyResourceVM = new ColonyResourceVM(blueprintResource, 0);
		AddDisposable(colonyResourceVM);
		ResourcesVMs.Add(colonyResourceVM);
		return colonyResourceVM;
	}

	private void UpdateStatus(Quest quest, bool forceComplete = false)
	{
		if (forceComplete)
		{
			IsNew = false;
			IsCompleted = true;
			IsUpdated = false;
			IsPostponed = false;
			IsFailed = false;
			return;
		}
		IsNew = quest.State == QuestState.Started;
		IsCompleted = quest.State == QuestState.Completed;
		IsPostponed = quest.State == QuestState.Postponed;
		IsFailed = quest.State == QuestState.Failed;
		IsUpdated = quest.IsViewed && ActiveObjectives.Any((QuestObjective o) => !o.IsViewed) && quest.State != QuestState.Completed && quest.State != QuestState.Failed;
	}

	private static int Comparison(QuestObjective o1, QuestObjective o2)
	{
		if (o1.State == o2.State)
		{
			return o2.Order.CompareTo(o1.Order);
		}
		return o1.State.CompareTo(o2.State);
	}

	private void UpdateCanCompleteState()
	{
		if (Requirements == null)
		{
			return;
		}
		bool flag = false;
		foreach (Requirement component in Quest.Blueprint.GetComponents<Requirement>())
		{
			flag = component.Check();
			if (!flag)
			{
				break;
			}
		}
		CanCompleteOrder = IsOrder && flag;
	}

	public void SelectQuest()
	{
		m_SelectQuestCallback?.Invoke(Quest);
		UpdateData();
	}

	private void OnSelectedQuestChanged(Quest quest)
	{
		if (quest != null)
		{
			IsSelected.Value = Quest == quest;
			UpdateCanCompleteState();
			UpdateData();
			UpdateStatus(Quest);
		}
	}

	public void CompleteOrder()
	{
		Game.Instance.GameCommandQueue.CompleteContract(Quest.Blueprint);
		UpdateStatus(Quest, forceComplete: true);
		UpdateData();
		IsOrderCompleted.Value = true;
		EventBus.RaiseEvent(delegate(IUpdateCanCompleteOrderNotificationHandler h)
		{
			h.HandleUpdateCanCompleteOrderNotificationInJournal();
		});
	}
}
