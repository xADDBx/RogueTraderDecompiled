using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationSpaceResourcesWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ExplorationSpaceResourcesVM ExplorationSpaceResourcesVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Exploration | ExplorationUISection.Colony | ExplorationUISection.ColonyProjects;

	public ExplorationSpaceResourcesWrapperVM()
	{
		AddDisposable(ExplorationSpaceResourcesVM = new ExplorationSpaceResourcesVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ExplorationSpaceResourcesVM.SetAdditionalResources(colony);
	}
}
