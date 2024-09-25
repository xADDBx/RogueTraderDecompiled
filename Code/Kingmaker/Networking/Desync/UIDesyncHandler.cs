using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Networking.Hash;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;

namespace Kingmaker.Networking.Desync;

public class UIDesyncHandler : IDesyncHandler
{
	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.DesyncWasDetected, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes)
			{
				EventBus.RaiseEvent(delegate(INetLobbyRequest h)
				{
					h.HandleNetLobbyRequest();
				});
			}
		});
	}
}
