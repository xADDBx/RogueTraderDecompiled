using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/MapObjectTransform")]
[AllowMultipleComponents]
[TypeId("5c44697f335b9a649b5169c6c9901d8d")]
public class MapObjectTransform : TransformEvaluator
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	public EntityReference MapObject;

	protected override Transform GetValueInternal()
	{
		MapObjectView mapObjectView = MapObject.FindView() as MapObjectView;
		if ((bool)mapObjectView)
		{
			return ObjectExtensions.Or(mapObjectView, null).gameObject.transform;
		}
		return null;
	}

	public override string GetCaption()
	{
		return MapObject?.ToString() ?? "";
	}
}
