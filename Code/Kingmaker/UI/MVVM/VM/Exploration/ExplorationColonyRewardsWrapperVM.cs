using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyRewardsWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyRewardsVM ColonyRewardsVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyRewardsWrapperVM()
	{
		AddDisposable(ColonyRewardsVM = new ColonyRewardsVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyRewardsVM.SetColony(colony);
	}
}
