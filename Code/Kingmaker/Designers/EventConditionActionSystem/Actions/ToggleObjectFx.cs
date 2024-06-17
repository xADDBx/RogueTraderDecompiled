using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("bf4b7db0186cac54189211f9684889a1")]
public class ToggleObjectFx : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator Target;

	public bool ToggleOn;

	public override void RunAction()
	{
		if ((bool)Target)
		{
			MapObjectFxPart mapObjectFxPart = Target.GetValue()?.GetOptional<MapObjectFxPart>();
			if ((bool)mapObjectFxPart)
			{
				mapObjectFxPart.SetFxActive(ToggleOn);
			}
		}
	}

	public override string GetCaption()
	{
		return string.Format("Set FX on ({0}) to {1}", Target?.GetCaption() ?? "None", ToggleOn ? "ON" : "OFF");
	}
}
