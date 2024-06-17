using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyProjectsWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyProjectsVM ColonyProjectsVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.ColonyProjects;

	public ExplorationColonyProjectsWrapperVM()
	{
		AddDisposable(ColonyProjectsVM = new ColonyProjectsVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	public void ClearNavigation()
	{
		ColonyProjectsVM.ClearNavigation();
	}

	private void SetColony(Colony colony)
	{
		ColonyProjectsVM.SetColony(colony);
	}
}
