using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
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

	protected override void RunAction()
	{
		InteractionLootPart optional = MapObject.GetValue().GetOptional<InteractionLootPart>();
		BaseUnitEntity baseUnitEntity = ContextData<InteractingUnitData>.Current?.Unit;
		baseUnitEntity = baseUnitEntity ?? Game.Instance.Player.MainCharacterEntity;
		optional?.Interact(baseUnitEntity);
	}
}
