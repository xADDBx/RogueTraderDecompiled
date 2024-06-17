using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(true)]
[TypeId("2376c5a54e08498e84849985dd09e8c9")]
public class IsScriptZoneActive : Condition
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	protected override string GetConditionCaption()
	{
		return $"Is script zone active ({ScriptZone})";
	}

	protected override bool CheckCondition()
	{
		ScriptZone scriptZone = ScriptZone.FindView() as ScriptZone;
		if ((bool)scriptZone)
		{
			return scriptZone.IsActive;
		}
		return false;
	}
}
