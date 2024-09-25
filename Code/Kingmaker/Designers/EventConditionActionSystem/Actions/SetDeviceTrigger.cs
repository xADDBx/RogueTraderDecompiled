using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetDeviceTrigger")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("5998acd801b48b34a9dfb61866f358ba")]
public class SetDeviceTrigger : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator Device;

	public string Trigger;

	public override string GetCaption()
	{
		return $"Set device trigger ({Device}:{Trigger})";
	}

	protected override void RunAction()
	{
		Device.GetValue().GetOptional<InteractionDevicePart>()?.SetTrigger(Trigger);
	}
}
