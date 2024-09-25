using Kingmaker.Items.Slots;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickWeaponSet : ITooltipBrick
{
	private readonly WeaponSlot m_WeaponSlot;

	public TooltipBrickWeaponSet(HandSlot handSlot, bool isPrimary)
	{
		m_WeaponSlot = (isPrimary ? handSlot.HandsEquipmentSet.PrimaryHand : handSlot.HandsEquipmentSet.SecondaryHand);
	}

	public TooltipBrickWeaponSet(WeaponSlot weapon)
	{
		m_WeaponSlot = weapon;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickWeaponSetVM(m_WeaponSlot);
	}
}
