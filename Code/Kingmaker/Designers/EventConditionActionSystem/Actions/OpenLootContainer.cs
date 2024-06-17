using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/OpenLootContainer")]
[AllowMultipleComponents]
[TypeId("de11b243ffe6dbe449ab5340f716ab26")]
public class OpenLootContainer : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return $"Пытается открыть лут из мапобжекта {MapObject}";
	}

	public override string GetCaption()
	{
		return "Open Loot Container " + MapObject;
	}

	public override void RunAction()
	{
		MapObject.GetValue().GetOptional<InteractionLootPart>()?.Interact(Game.Instance.Player.MainCharacterEntity);
	}
}
