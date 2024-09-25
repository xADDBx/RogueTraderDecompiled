using System;
using Kingmaker.Networking;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class GamerTagAndNameVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<string> UserId;

	public readonly ReactiveProperty<PhotonActorNumber> UserNumber;

	public readonly ReactiveProperty<string> Name;

	public GamerTagAndNameVM(ReactiveProperty<string> userId, ReactiveProperty<PhotonActorNumber> userNumber, ReactiveProperty<string> name)
	{
		UserId = userId;
		UserNumber = userNumber;
		Name = name;
	}

	protected override void DisposeImplementation()
	{
	}

	public async void ShowGamerCard()
	{
		PFLog.UI.Log($"Show card User Id {UserId.Value} / User Number {UserNumber.Value} / Name {Name.Value}");
	}
}
