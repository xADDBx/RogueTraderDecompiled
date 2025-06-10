using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("576a514e5164d254f936cd4ce788ae6a")]
public class RunActionHolder : GameAction
{
	public string Comment;

	[ShowCreator]
	public ActionsReference Holder;

	public override string GetCaption()
	{
		return "Run Action Holder (" + Comment + " )";
	}

	protected override void RunAction()
	{
		Holder.Get()?.Actions.Run();
	}
}
