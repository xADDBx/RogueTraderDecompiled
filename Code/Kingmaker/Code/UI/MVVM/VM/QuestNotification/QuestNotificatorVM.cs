using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.QuestNotification;

public class QuestNotificatorVM : VMBase, INewServiceWindowUIHandler, ISubscriber, IQuestObjectiveHandler, IQuestHandler
{
	public static QuestNotificatorVM Instance;

	public readonly ReactiveProperty<bool> IsShowUp = new ReactiveProperty<bool>(initialValue: false);

	public readonly AutoDisposingReactiveCollection<QuestNotificationQuestVM> QuestEntities = new AutoDisposingReactiveCollection<QuestNotificationQuestVM>();

	public readonly AutoDisposingReactiveCollection<QuestNotificationEntityVM> ObjectiveEntities = new AutoDisposingReactiveCollection<QuestNotificationEntityVM>();

	public readonly ReactiveCommand ForceCloseCommand = new ReactiveCommand();

	public readonly ReactiveCommand ClearCommand = new ReactiveCommand();

	public bool ForbiddenQuestNotification
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.Cutscene) && !LoadingProcess.Instance.IsLoadingScreenActive)
			{
				return RootUIContext.Instance.FullScreenUIType == FullScreenUIType.GroupChanger;
			}
			return true;
		}
	}

	public QuestNotificatorVM()
	{
		EventBus.Subscribe(this);
		Instance = this;
	}

	public void HandleQuestStarted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.New);
	}

	public void HandleQuestCompleted(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Completed);
	}

	public void HandleQuestFailed(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Failed);
	}

	public void HandleQuestUpdated(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Updated);
	}

	public void HandleQuestPostponed(Quest quest)
	{
		AddQuestNotification(quest, QuestNotificationState.Postponed);
	}

	private void AddQuestNotification(Quest quest, QuestNotificationState state)
	{
		if (!quest.Blueprint.IsSilentQuestNotification(state))
		{
			QuestEntities.Add(new QuestNotificationQuestVM(quest, state));
		}
	}

	public void HandleQuestShowed(QuestNotificationQuestVM quest)
	{
		quest.Dispose();
		QuestEntities.Remove(quest);
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.New);
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.Completed);
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		AddObjective(objective, QuestNotificationState.Failed);
	}

	private void AddObjective(QuestObjective objective, QuestNotificationState state)
	{
		if (objective.IsVisible && !objective.Blueprint.IsSilentQuestNotification(state))
		{
			QuestNotificationEntityVM questNotificationEntityVM = new QuestNotificationEntityVM(objective, state);
			QuestNotificationEntityVM questNotificationEntityVM2 = ObjectiveEntities.FirstOrDefault((QuestNotificationEntityVM o) => !o.IsAddendum && o.Quest == objective.Quest);
			if (questNotificationEntityVM2 != null && !objective.Blueprint.IsAddendum)
			{
				questNotificationEntityVM2.AddObjective(questNotificationEntityVM);
			}
			else
			{
				ObjectiveEntities.Add(questNotificationEntityVM);
			}
		}
	}

	public void HandleObjectiveShowed(QuestNotificationEntityVM objective)
	{
		objective.Dispose();
		ObjectiveEntities.Remove(objective);
	}

	public void ForceClose()
	{
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
		IsShowUp.Value = false;
		ForceCloseCommand.Execute();
	}

	public void HandleOpenJournal()
	{
		ForceClose();
	}

	public void HandleCloseAll()
	{
	}

	public void HandleOpenWindowOfType(ServiceWindowsType type)
	{
	}

	public void HandleOpenInventory()
	{
	}

	public void HandleOpenEncyclopedia(INode page = null)
	{
	}

	public void HandleOpenCharacterInfo()
	{
	}

	public void HandleOpenCharacterInfoPage(CharInfoPageType pageType, BaseUnitEntity unitEntity)
	{
	}

	public void HandleOpenShipCustomizationPage(ShipCustomizationTab pageType)
	{
	}

	public void HandleOpenLocalMap()
	{
	}

	public void HandleOpenColonyManagement()
	{
	}

	public void HandleOpenCargoManagement()
	{
	}

	public void HandleOpenShipCustomization(bool force = false)
	{
	}

	public void OnAreaBeginUnloading()
	{
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
		ClearCommand.Execute();
	}

	protected override void DisposeImplementation()
	{
		Instance = null;
		QuestEntities.Clear();
		ObjectiveEntities.Clear();
		EventBus.Unsubscribe(this);
	}
}
