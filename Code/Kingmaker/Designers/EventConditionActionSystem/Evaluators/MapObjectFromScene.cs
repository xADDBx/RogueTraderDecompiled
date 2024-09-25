using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("2f7f1ee24f13dca42977f3b2b5a847f8")]
[PlayerUpgraderAllowed(true)]
public class MapObjectFromScene : MapObjectEvaluator
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	public EntityReference MapObject;

	protected override MapObjectEntity GetMapObjectInternal()
	{
		MapObjectView mapObjectView = MapObject.FindView() as MapObjectView;
		if (mapObjectView != null)
		{
			return mapObjectView.Data;
		}
		return null;
	}

	public override string GetCaption()
	{
		return "Object from scene " + MapObject;
	}
}
