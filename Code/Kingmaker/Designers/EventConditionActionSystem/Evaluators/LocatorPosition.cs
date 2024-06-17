using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/LocatorPosition")]
[AllowMultipleComponents]
[TypeId("ee272e7d88aff6648b4c1b052228fdc7")]
public class LocatorPosition : PositionEvaluator
{
	[AllowedEntityType(typeof(LocatorView))]
	[ValidateNotEmpty]
	public EntityReference Locator;

	public Vector3 Offset;

	protected override Vector3 GetValueInternal()
	{
		return (Locator.FindView().Or(null)?.GO.transform.position + Offset) ?? throw new FailToEvaluateException(this);
	}

	public override string GetCaption()
	{
		return Locator.EntityNameInEditor;
	}
}
