using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Stats;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyStatsWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyStatsVM ColonyStatsVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyStatsWrapperVM()
	{
		AddDisposable(ColonyStatsVM = new ColonyStatsVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyStatsVM.SetColony(colony);
	}
}
