using System;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.SystemMap;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISetCurrentQuestHandler, ISubscriber
{
	public readonly ReactiveProperty<Quest> SelectedQuest = new ReactiveProperty<Quest>();

	public readonly JournalNavigationVM Navigation;

	public readonly LensSelectorVM Selector;

	public readonly SystemMapSpaceResourcesVM SystemMapSpaceResourcesVM;

	public readonly ReactiveCommand<Quest> UpdateView = new ReactiveCommand<Quest>();

	public JournalVM()
	{
		AddDisposable(Navigation = new JournalNavigationVM(GameHelper.Quests.GetList(), SelectedQuest, SelectQuest));
		AddDisposable(Selector = new LensSelectorVM());
		AddDisposable(SystemMapSpaceResourcesVM = new SystemMapSpaceResourcesVM());
		AddDisposable(EventBus.Subscribe(this));
		SelectedQuest.Value = JournalHelper.CurrentQuest ?? SelectedQuest.Value;
		UpdateView.Execute(SelectedQuest.Value);
	}

	private void SelectQuest(Quest quest)
	{
		if (quest != null)
		{
			SelectedQuest.Value = quest;
			UpdateView.Execute(SelectedQuest.Value);
			JournalHelper.ChangeCurrentQuest(quest);
		}
	}

	protected override void DisposeImplementation()
	{
	}

	void ISetCurrentQuestHandler.HandleSetCurrentQuest(Quest quest)
	{
		SelectedQuest.Value = quest;
		UpdateView.Execute(SelectedQuest.Value);
	}
}
