namespace Kingmaker.View.Equipment;

public class WeaponSet
{
	public readonly UnitViewHandSlotData MainHand;

	public readonly UnitViewHandSlotData OffHand;

	public WeaponSet(UnitViewHandSlotData mainHand, UnitViewHandSlotData offHand)
	{
		MainHand = mainHand;
		OffHand = offHand;
	}
}
