using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickProtocolPet : ITooltipBrick
{
	public ItemEntity Protocol;

	public TooltipBrickProtocolPet(EquipmentSlot<BlueprintItemEquipmentPetProtocol> protocolSlot)
	{
		Protocol = protocolSlot.Item;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickProtocolPetVM(Protocol);
	}
}
