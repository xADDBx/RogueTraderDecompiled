using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.SelectionGroup;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

public class CharGenPortraitTabVM : SelectionGroupEntityVM, INetLobbyPlayersHandler, ISubscriber
{
	public readonly CharGenPortraitTab Tab;

	public readonly ReactiveCommand<bool> CheckCoopControls = new ReactiveCommand<bool>();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	public CharGenPortraitTabVM(CharGenPortraitTab tab)
		: base(allowSwitchOff: false)
	{
		Tab = tab;
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DoSelectMe()
	{
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		CheckCoopControls.Execute(UINetUtility.IsControlMainCharacter());
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
