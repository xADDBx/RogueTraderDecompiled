using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Conditional")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("52d8973f2e470e14c97b74209680491a")]
public class Conditional : GameAction
{
	public string Comment;

	public ConditionsChecker ConditionsChecker;

	public ActionList IfTrue;

	public ActionList IfFalse;

	public override string GetDescription()
	{
		return "Позволяет добавить условия в последовательность действий";
	}

	protected override void RunAction()
	{
		(ConditionsChecker.Check(null, @unsafe: true) ? IfTrue : IfFalse).Run(@unsafe: true);
	}

	public override string GetCaption()
	{
		return "Conditional (" + Comment + " )";
	}
}
