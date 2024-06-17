using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/DestroyMapObject")]
[AllowMultipleComponents]
[TypeId("10b37ccc7a0511b4ba6c4cbf72b22f76")]
public class DestroyMapObject : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override void RunAction()
	{
		MapObjectEntity mapObjectEntity = (MapObject ? MapObject.GetValue() : null);
		if (mapObjectEntity == null)
		{
			PFLog.Default.Error(this, "Cannot find map object");
			return;
		}
		DestructionPart optional = mapObjectEntity.GetOptional<DestructionPart>();
		if (optional == null)
		{
			PFLog.Default.Error(this, "Map object " + mapObjectEntity.View?.name + " does not have an Destruction component");
		}
		else
		{
			optional.ForceDestroy();
		}
	}

	public override string GetCaption()
	{
		return "Destroy map-object " + MapObject?.GetCaption();
	}
}
