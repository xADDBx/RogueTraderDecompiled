using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("6b1117fd331ad53419ba9a408e756209")]
public class WarhammerContexActionChangeWeaponSet : ContextAction
{
	protected override void RunAction()
	{
		PartUnitBody partUnitBody = base.Target?.Entity?.GetBodyOptional();
		if (partUnitBody != null)
		{
			int currentHandEquipmentSetIndex = partUnitBody.CurrentHandEquipmentSetIndex;
			partUnitBody.CurrentHandEquipmentSetIndex = (currentHandEquipmentSetIndex + 1) % partUnitBody.HandsEquipmentSets.Count;
		}
	}

	public override string GetCaption()
	{
		return "Change weapon set";
	}
}
