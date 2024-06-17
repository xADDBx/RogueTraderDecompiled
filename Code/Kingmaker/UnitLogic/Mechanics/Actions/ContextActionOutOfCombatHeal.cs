using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.QA;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("01d1e01cb796456aab8dcde93f0e0b6d")]
public class ContextActionOutOfCombatHeal : ContextAction
{
	public override string GetCaption()
	{
		return "Deprecated, use ContextActionMedicae instead";
	}

	public override void RunAction()
	{
		PFLog.Default.ErrorWithReport("ContextActionOutOfCombatHeal is deprecated, use ContextActionMedicae instead");
	}
}
