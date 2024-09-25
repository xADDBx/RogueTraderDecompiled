using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("df903088c314c0349ab3b5dda20abb9c")]
public class LocatorTransform : TransformEvaluator
{
	[AllowedEntityType(typeof(LocatorView))]
	[ValidateNotEmpty]
	public EntityReference Locator;

	protected override Transform GetValueInternal()
	{
		return Locator.FindView().Or(null)?.ViewTransform;
	}

	public override string GetCaption()
	{
		return Locator?.ToString() ?? "";
	}
}
