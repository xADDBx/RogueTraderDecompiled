using Kingmaker.UI.MVVM.VM.NetLobby;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbyInvitePlayerDifferentPlatformsBaseView : ViewBase<NetLobbyInvitePlayerDifferentPlatformsVM>
{
	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}
}
