using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/False")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("f3e94df96a3153f4eb5a5c97dfa322e8")]
public class False : Condition
{
	protected override string GetConditionCaption()
	{
		return "False";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
