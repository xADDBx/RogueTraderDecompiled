using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("01d1e01cb796456aab8dcde93f0e0b6d")]
public class ContextActionOutOfCombatHeal : ContextAction
{
	public override string GetCaption()
	{
		return "Deprecated, use ContextActionMedicae instead";
	}

	protected override void RunAction()
	{
		Element.LogError(this, "ContextActionOutOfCombatHeal is deprecated, use ContextActionMedicae instead");
	}
}
