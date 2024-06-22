using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("6daf9ce41e164e3f80752d49caa70da5")]
public class ContextActionPropheticIntervention : ContextAction
{
	protected override void RunAction()
	{
		UnitEntity target = base.Context.MainTarget.Entity as UnitEntity;
		UnitEntity obj = base.Context.MaybeCaster as UnitEntity;
		UnitPartPropheticIntervention unitPartPropheticIntervention = obj?.Parts.GetOptional<UnitPartPropheticIntervention>();
		if (obj != null && target != null && unitPartPropheticIntervention != null && unitPartPropheticIntervention.Entries.Any((UnitPartPropheticIntervention.PropheticInterventionEntry p) => p.DeadTarget == target))
		{
			UnitPartPropheticIntervention.PropheticInterventionEntry propheticInterventionEntry = unitPartPropheticIntervention.Entries.Where((UnitPartPropheticIntervention.PropheticInterventionEntry p1) => p1.DeadTarget == target).MinBy((UnitPartPropheticIntervention.PropheticInterventionEntry p2) => p2.WoundsBeforeLastAttack);
			target.LifeState.Resurrect(propheticInterventionEntry.WoundsBeforeLastAttack);
		}
	}

	public override string GetCaption()
	{
		return "Activate Prophetic Intervention on target - resurrect it if it died last turn";
	}
}
