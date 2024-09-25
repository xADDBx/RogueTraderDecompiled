using System;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.UI.MVVM.VM.Colonization.Stats;
using Kingmaker.UI.MVVM.VM.Colonization.Traits;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;

public class ColonyManagementPageVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ColonyRewardsVM ColonyRewardsVM;

	public readonly ColonyStatsVM ColonyStatsVM;

	public readonly ColonyTraitsVM ColonyTraitsVM;

	public readonly ColonyEventsVM ColonyEventsVM;

	public readonly ExplorationSpaceResourcesVM ExplorationSpaceResourcesVM;

	public readonly ColonyProjectsVM ColonyProjectsVM;

	public readonly ColonyProjectsButtonVM ColonyProjectsButtonVM;

	public readonly ColonyProjectsBuiltListVM ColonyProjectsBuiltListVM;

	public Colony Colony { get; }

	public ColonyManagementPageVM(Colony colony)
	{
		Colony = colony;
		AddDisposable(ColonyRewardsVM = new ColonyRewardsVM());
		AddDisposable(ColonyStatsVM = new ColonyStatsVM());
		AddDisposable(ColonyTraitsVM = new ColonyTraitsVM());
		AddDisposable(ColonyEventsVM = new ColonyEventsVM());
		AddDisposable(ExplorationSpaceResourcesVM = new ExplorationSpaceResourcesVM());
		AddDisposable(ColonyProjectsVM = new ColonyProjectsVM());
		AddDisposable(ColonyProjectsButtonVM = new ColonyProjectsButtonVM());
		AddDisposable(ColonyProjectsBuiltListVM = new ColonyProjectsBuiltListVM());
		ColonyRewardsVM.SetColony(colony, isColonyManagement: true);
		ColonyStatsVM.SetColony(colony, isColonyManagement: true);
		ColonyTraitsVM.SetColony(colony, isColonyManagement: true);
		ColonyEventsVM.SetColony(colony, isColonyManagement: true);
		ExplorationSpaceResourcesVM.SetAdditionalResources(colony);
		ColonyProjectsVM.SetColony(colony, isColonyManagement: true);
		ColonyProjectsButtonVM.SetColony(colony, isColonyManagement: true);
		ColonyProjectsBuiltListVM.SetColony(colony, isColonyManagement: true);
	}

	protected override void DisposeImplementation()
	{
		ColonyProjectsVM.ClearNavigation();
	}
}
