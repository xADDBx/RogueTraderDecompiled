using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Stats;

public class ColonyStatsVM : ColonyUIComponentVM, IColonyStatsHandler, ISubscriber
{
	public readonly AutoDisposingReactiveCollection<ColonyStatVM> StatVMs = new AutoDisposingReactiveCollection<ColonyStatVM>();

	public readonly ReactiveCommand UpdateStatsCommand = new ReactiveCommand();

	public ColonyStatsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void SetColonyImpl(Colony colony)
	{
		AddStats();
	}

	public void HandleColonyStatsChanged()
	{
		UpdateStats();
	}

	private void AddStats()
	{
		StatVMs.Clear();
		if (m_Colony == null)
		{
			return;
		}
		AddStatVM(ColonyStatType.Efficiency);
		AddStatVM(ColonyStatType.Contentment);
		AddStatVM(ColonyStatType.Security);
		foreach (ColonyStatVM statVM in StatVMs)
		{
			statVM.UpdateStat(m_Colony);
		}
		UpdateStatsCommand.Execute();
	}

	private void UpdateStats()
	{
		foreach (ColonyStatVM statVM in StatVMs)
		{
			statVM.UpdateStat(m_Colony);
		}
	}

	private void AddStatVM(ColonyStatType colonyStatType)
	{
		ColonyStatVM colonyStatVM = new ColonyStatVM(m_Colony, colonyStatType);
		AddDisposable(colonyStatVM);
		StatVMs.Add(colonyStatVM);
	}
}
