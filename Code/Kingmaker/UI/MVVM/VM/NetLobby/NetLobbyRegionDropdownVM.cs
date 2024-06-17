using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyRegionDropdownVM : DropdownItemVM
{
	public readonly string Region;

	public NetLobbyRegionDropdownVM(string text, string region, Sprite icon = null)
		: base(text, icon)
	{
		Region = region;
	}
}
