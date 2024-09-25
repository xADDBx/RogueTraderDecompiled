using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("ae3ad240503b4ac4a9a505a90ff05741")]
[PlayerUpgraderAllowed(false)]
public class DebugLog : GameAction
{
	public string Log;

	public override string GetDescription()
	{
		return "Пишет сообщение в лог UberConsole";
	}

	public override string GetCaption()
	{
		return "Log: " + Log;
	}

	protected override void RunAction()
	{
		Element.LogInfo(Log);
	}
}
