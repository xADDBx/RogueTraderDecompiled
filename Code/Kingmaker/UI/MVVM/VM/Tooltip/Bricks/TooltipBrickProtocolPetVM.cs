using Kingmaker.Blueprints.Items;
using Kingmaker.Items;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickProtocolPetVM : TooltipBaseBrickVM
{
	public Sprite ProtocolIcon;

	public string ProtocolName;

	public BlueprintItem Item;

	public TooltipBrickProtocolPetVM(ItemEntity protocol)
	{
		ProtocolIcon = protocol.Icon;
		ProtocolName = protocol.Name;
		Item = protocol.Blueprint;
	}
}
