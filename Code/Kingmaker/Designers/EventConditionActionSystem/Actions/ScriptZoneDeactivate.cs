using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/ScriptZoneDeactivate")]
[AllowMultipleComponents]
[TypeId("936004d258436d2459d0339955a70892")]
[PlayerUpgraderAllowed(true)]
public class ScriptZoneDeactivate : GameAction
{
	[AllowedEntityType(typeof(ScriptZone))]
	[ValidateNotEmpty]
	public EntityReference ScriptZone;

	public override void RunAction()
	{
		ScriptZone scriptZone = ScriptZone.FindView() as ScriptZone;
		if ((bool)scriptZone)
		{
			scriptZone.IsActive = false;
		}
	}

	public override string GetCaption()
	{
		return $"Script Zone Deactivate ({ScriptZone})";
	}
}
