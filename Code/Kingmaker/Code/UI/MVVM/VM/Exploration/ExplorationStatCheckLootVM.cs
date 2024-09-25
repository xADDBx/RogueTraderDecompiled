using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.Exploration;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationStatCheckLootVM : ExplorationComponentBaseVM
{
	public readonly StatCheckLootPointOfInterestVM StatCheckLootPointOfInterestVM;

	public ExplorationStatCheckLootVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StatCheckLootPointOfInterestVM = new StatCheckLootPointOfInterestVM());
	}
}
