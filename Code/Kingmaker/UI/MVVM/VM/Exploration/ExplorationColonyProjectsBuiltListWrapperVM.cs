using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyProjectsBuiltListWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyProjectsBuiltListVM ColonyProjectsBuiltListVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyProjectsBuiltListWrapperVM()
	{
		AddDisposable(ColonyProjectsBuiltListVM = new ColonyProjectsBuiltListVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyProjectsBuiltListVM.SetColony(colony);
	}
}
