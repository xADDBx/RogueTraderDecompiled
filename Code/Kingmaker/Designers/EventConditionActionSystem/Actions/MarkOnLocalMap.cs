using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/MarkOnLocalMap")]
[AllowMultipleComponents]
[TypeId("2e7785dc3e816b94da09d0151bcd34b0")]
public class MarkOnLocalMap : GameAction
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public bool Hidden;

	public override string GetDescription()
	{
		return string.Format("{0} маркер для мапобжекта {1} на локальной карте\n", Hidden ? "Выключает" : "Включает", MapObject) + "Для работы на мапобжекте должен висеть компонент LocalMapMarker";
	}

	public override void RunAction()
	{
		MapObjectEntity value = MapObject.GetValue();
		if (value != null)
		{
			LocalMapMarkerPart optional = value.GetOptional<LocalMapMarkerPart>();
			if ((bool)optional)
			{
				optional.SetHidden(Hidden);
				return;
			}
			PFLog.Default.Error("Cannot mark " + value?.ToString() + ": no LocalMapMarker component.", value);
		}
	}

	public override string GetCaption()
	{
		return string.Format("Make ({0}) {1} on map", MapObject, Hidden ? "hidden" : "marked");
	}
}
