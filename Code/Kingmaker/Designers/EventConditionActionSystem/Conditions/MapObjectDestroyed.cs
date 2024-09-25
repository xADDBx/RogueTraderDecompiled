using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("6c417a586b0776e4897b0054f29efa40")]
public class MapObjectDestroyed : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return "Is map object destroyed or not?";
	}

	protected override string GetConditionCaption()
	{
		return $"MapObject {MapObject} is destroyed";
	}

	protected override bool CheckCondition()
	{
		return MapObject.GetValue().GetOptional<DestructionPart>()?.AlreadyDestroyed ?? false;
	}
}
