using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("000982a9766f40b198b4be10465175b0")]
public class WarhammerResetStrangulate : ContextAction
{
	protected override void RunAction()
	{
		base.Context?.MaybeCaster?.GetOptional<WarhammerUnitPartStrangulateController>()?.RemoveAllBuffs();
	}

	public override string GetCaption()
	{
		return "Stops all current strangulate effects";
	}
}
