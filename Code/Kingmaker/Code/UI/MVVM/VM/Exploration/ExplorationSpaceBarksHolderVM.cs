using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.VM.Exploration;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class ExplorationSpaceBarksHolderVM : ExplorationComponentBaseVM, IExplorationUIHandler, ISubscriber
{
	public readonly SpaceBarksHolderVM SpaceBarksHolderVM;

	public ExplorationSpaceBarksHolderVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(SpaceBarksHolderVM = new SpaceBarksHolderVM());
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		SpaceBarksHolderVM.Show();
	}

	public void CloseExplorationScreen()
	{
		SpaceBarksHolderVM.Hide();
	}
}
