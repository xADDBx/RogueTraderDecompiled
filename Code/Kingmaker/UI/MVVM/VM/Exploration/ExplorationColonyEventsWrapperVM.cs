using Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Colonization.Events;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Exploration;

public class ExplorationColonyEventsWrapperVM : ExplorationUIComponentWrapperVM
{
	public readonly ColonyEventsVM ColonyEventsVM;

	private static StarSystemObjectStateVM StarSystemObjectStateVM => StarSystemObjectStateVM.Instance;

	protected override ExplorationUISection ExplorationUISection => ExplorationUISection.Colony;

	public ExplorationColonyEventsWrapperVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ColonyEventsVM = new ColonyEventsVM());
		AddDisposable(StarSystemObjectStateVM.Colony.Subscribe(SetColony));
	}

	private void SetColony(Colony colony)
	{
		ColonyEventsVM.SetColony(colony);
	}
}
