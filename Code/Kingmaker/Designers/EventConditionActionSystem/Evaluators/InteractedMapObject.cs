using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("3c3f22e88cd3e274bae4026a0ba70c6e")]
public class InteractedMapObject : MapObjectEvaluator
{
	public override string GetCaption()
	{
		return "Interacted MapObject";
	}

	protected override MapObjectEntity GetMapObjectInternal()
	{
		return MechanicEntityData.CurrentMapObject;
	}
}
