using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("bc2f10222d7a4dcab27f18e539e160fd")]
public class EvaluatedMapObjectTransform : TransformEvaluator
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	protected override Transform GetValueInternal()
	{
		return (MapObject ? MapObject.GetValue() : null)?.View.ViewTransform;
	}

	public override string GetCaption()
	{
		return MapObject?.ToString() ?? "";
	}
}
