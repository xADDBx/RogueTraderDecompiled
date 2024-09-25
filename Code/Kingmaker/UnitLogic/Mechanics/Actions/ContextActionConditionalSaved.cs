using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d61949c589ee885458c9439b2aa202b6")]
public class ContextActionConditionalSaved : ContextAction
{
	public ActionList Succeed;

	public ActionList Failed;

	public override string GetCaption()
	{
		return "Conditional saved";
	}

	protected override void RunAction()
	{
		RulePerformSavingThrow savingThrow = base.Context.SavingThrow;
		if (savingThrow == null)
		{
			Element.LogError(this, "Can't use ContextActionConditionalSaved if no saving throw rolled");
		}
		else if (savingThrow.IsPassed)
		{
			Succeed.Run();
		}
		else
		{
			Failed.Run();
		}
	}
}
