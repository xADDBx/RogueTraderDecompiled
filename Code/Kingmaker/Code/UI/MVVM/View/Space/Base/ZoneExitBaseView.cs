using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.Space.Base;

public class ZoneExitBaseView : ViewBase<ZoneExitVM>, IAreaHandler, ISubscriber
{
	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (Game.Instance.Player.IsForceOpenVoidshipUpgrade && base.ViewModel.HasAccessStarshipInventory.Value)
		{
			base.ViewModel.OpenShipCustomization();
			Game.Instance.Player.IsForceOpenVoidshipUpgrade = false;
		}
	}
}
