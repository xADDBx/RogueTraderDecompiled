using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.SystemMap;

public class SystemMapVM : CommonStaticComponentVM, INetRoleSetHandler, ISubscriber, INetStopPlayingHandler
{
	public readonly StarSystemSpaceBarksHolderVM StarSystemSpaceBarksHolderVM;

	public readonly ReactiveProperty<bool> IsControllable = new ReactiveProperty<bool>(initialValue: true);

	public SystemMapVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StarSystemSpaceBarksHolderVM = new StarSystemSpaceBarksHolderVM());
		IsControllable.Value = UINetUtility.IsControlMainCharacter();
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleRoleSet(string entityId)
	{
		IsControllable.Value = UINetUtility.IsControlMainCharacter();
	}

	public void HandleStopPlaying()
	{
		IsControllable.Value = UINetUtility.IsControlMainCharacter();
	}
}
