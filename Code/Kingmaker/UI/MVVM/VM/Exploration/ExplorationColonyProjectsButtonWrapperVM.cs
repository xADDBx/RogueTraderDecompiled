using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyProjectsButtonWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyProjectsButtonVM ColonyProjectsButtonVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyProjectsButtonWrapperVM()
	{
		AddDisposable(ColonyProjectsButtonVM = new ColonyProjectsButtonVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyProjectsButtonVM.SetColony(colony);
	}
}
