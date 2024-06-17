using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("ecfe0525066a4884a72ec6d06dc2f016")]
public class ContextActionRespecCaster : ContextAction
{
	public override string GetCaption()
	{
		return "Force respec";
	}

	public override void RunAction()
	{
		if (base.Context?.MaybeCaster is UnitEntity { Progression: not null } unitEntity && PartUnitProgression.CanRespec(unitEntity))
		{
			Game.Instance.GameCommandQueue.FinishRespec(unitEntity, forFree: true);
		}
		else
		{
			PFLog.Default.Error(this, $"Can't force respec {base.Context?.MaybeCaster}");
		}
	}
}
