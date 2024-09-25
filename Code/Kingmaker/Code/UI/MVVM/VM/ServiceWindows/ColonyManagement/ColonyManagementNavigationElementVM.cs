using System;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;

public class ColonyManagementNavigationElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly string Title;

	public readonly ColonyEventsVM ColonyEventsVM;

	private readonly Colony m_Colony;

	public ColonyManagementNavigationElementVM(Colony colony)
	{
		m_Colony = colony;
		Title = colony.Blueprint.Name.Text;
		AddDisposable(ColonyEventsVM = new ColonyEventsVM());
		ColonyEventsVM.SetColony(colony, isColonyManagement: true);
	}

	public void SelectPage()
	{
		Game.Instance.GameCommandQueue.SelectColony(m_Colony);
	}

	public void SetSelection(Colony colony)
	{
		IsSelected.Value = m_Colony == colony;
	}

	protected override void DisposeImplementation()
	{
	}
}
