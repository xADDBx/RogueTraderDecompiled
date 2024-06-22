using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("d8f70246d92496c4ba12ae49d9876614")]
public class ContextActionRepeat : ContextAction
{
	public ActionList Actions;

	public ContextValue RepeatNumber;

	public override string GetCaption()
	{
		return "Repeats action several times";
	}

	protected override void RunAction()
	{
		int num = RepeatNumber.Calculate(base.Context);
		for (int i = 0; i < num; i++)
		{
			Actions.Run();
		}
	}
}
