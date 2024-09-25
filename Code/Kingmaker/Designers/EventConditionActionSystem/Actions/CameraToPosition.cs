using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/CameraToPosition")]
[AllowMultipleComponents]
[TypeId("1a0449d4049c34149a17869dd62dc64a")]
public class CameraToPosition : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator Position;

	public override string GetDescription()
	{
		return $"Моментально перемещает камеру в указанную позицию {Position}";
	}

	protected override void RunAction()
	{
		CameraRig.Instance.ScrollTo(Position.GetValue());
	}

	public override string GetCaption()
	{
		return $"Camera To Position ({Position})";
	}
}
