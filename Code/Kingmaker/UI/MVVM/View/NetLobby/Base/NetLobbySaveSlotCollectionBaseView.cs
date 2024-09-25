using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;

namespace Kingmaker.UI.MVVM.View.NetLobby.Base;

public class NetLobbySaveSlotCollectionBaseView : SaveSlotCollectionVirtualBaseView
{
	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.gameObject.SetActive(value: true);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		base.gameObject.SetActive(value: false);
	}
}
