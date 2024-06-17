using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ScriptZoneActivate")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("c99aa15b0ad07ce4db8044f7adfccaa5")]
public class ScriptZoneActivate : GameAction
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	public override void RunAction()
	{
		ScriptZone scriptZone = ScriptZone.FindView() as ScriptZone;
		if ((bool)scriptZone)
		{
			scriptZone.IsActive = true;
		}
	}

	public override string GetCaption()
	{
		return $"Script Zone Activate ({ScriptZone})";
	}
}
