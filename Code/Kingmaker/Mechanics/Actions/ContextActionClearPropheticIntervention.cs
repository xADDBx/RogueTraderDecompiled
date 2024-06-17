using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Mechanics.Actions;

[TypeId("4017ffe5c10f497892af948b794a68b6")]
public class ContextActionClearPropheticIntervention : ContextAction
{
	public override void RunAction()
	{
		((base.Context.MaybeCaster as UnitEntity)?.Parts.GetOptional<UnitPartPropheticIntervention>())?.Entries.Clear();
	}

	public override string GetCaption()
	{
		return "Activate Prophetic Intervention on target - resurrect it if it died last turn";
	}
}
