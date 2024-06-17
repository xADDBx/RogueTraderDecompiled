using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Colonization.Traits;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyTraitsWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyTraitsVM ColonyTraitsVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyTraitsWrapperVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ColonyTraitsVM = new ColonyTraitsVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyTraitsVM.SetColony(colony);
	}
}
