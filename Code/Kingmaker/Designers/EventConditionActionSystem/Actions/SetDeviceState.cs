using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SetDeviceState")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("d1d531364174e9942b14123b6f890f18")]
public class SetDeviceState : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator Device;

	[ValidateNotNull]
	[SerializeReference]
	public IntEvaluator State;

	public override string GetCaption()
	{
		return $"Set device state ({Device}:{State})";
	}

	protected override void RunAction()
	{
		Device.GetValue().GetOptional<InteractionDevicePart>()?.SetState(State.GetValue());
	}
}
