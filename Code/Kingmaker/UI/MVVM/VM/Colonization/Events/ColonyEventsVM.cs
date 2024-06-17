using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Events;

public class ColonyEventsVM : ColonyUIComponentVM, IColonizationEventHandler, ISubscriber
{
	public readonly AutoDisposingReactiveCollection<ColonyEventVM> EventsVMs = new AutoDisposingReactiveCollection<ColonyEventVM>();

	public readonly ReactiveCommand UpdateEventsCommand = new ReactiveCommand();

	public ColonyEventsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void SetColonyImpl(Colony colony)
	{
		UpdateEvents();
	}

	public void HandleEventStarted(Colony colony, BlueprintColonyEvent colonyEvent)
	{
		UpdateEvents();
	}

	public void HandleEventFinished(Colony colony, BlueprintColonyEvent colonyEvent, BlueprintColonyEventResult result)
	{
		UpdateEvents();
	}

	private void UpdateEvents()
	{
		EventsVMs.Clear();
		if (m_Colony == null)
		{
			return;
		}
		foreach (BlueprintColonyEvent startedEvent in m_Colony.StartedEvents)
		{
			AddEventVM(startedEvent);
		}
		UpdateEventsCommand.Execute();
	}

	private void AddEventVM(BlueprintColonyEvent colonyEvent)
	{
		ColonyEventVM colonyEventVM = new ColonyEventVM(m_Colony, colonyEvent, m_IsColonyManagement);
		AddDisposable(colonyEventVM);
		EventsVMs.Add(colonyEventVM);
	}
}
