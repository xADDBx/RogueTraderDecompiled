using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SwitchDoor")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("da1815e1ca093d14190aa9351c0c0f66")]
public class SwitchDoor : GameAction
{
	[AllowedEntityType(typeof(MapObjectView))]
	[ValidateNotEmpty]
	[SerializeReference]
	public MapObjectEvaluator Door;

	public bool UnlockIfLocked;

	public bool CloseIfAlreadyOpen;

	public bool OpenIfAlreadyClosed;

	public override string GetDescription()
	{
		return $"Пытается открыть или закрыть дверь. При попытке может разблокировать дверь. С дверью ничего не произойдет, если не стоит ни одной галки.\nДверь: {Door}\nРазблокировать ли дверь: {UnlockIfLocked}\nЗакрывать ли открытую дверь: {CloseIfAlreadyOpen}\nОткрывать ли закрытую дверь: {OpenIfAlreadyClosed}";
	}

	public override void RunAction()
	{
		InteractionDoorPart optional = Door.GetValue().GetOptional<InteractionDoorPart>();
		if (!optional)
		{
			return;
		}
		if (UnlockIfLocked)
		{
			optional.AlreadyUnlocked = true;
		}
		if (optional.GetState())
		{
			if (CloseIfAlreadyOpen)
			{
				optional.Open();
			}
		}
		else if (OpenIfAlreadyClosed)
		{
			optional.Open();
		}
	}

	public override string GetCaption()
	{
		return $"Switch door {Door}";
	}
}
