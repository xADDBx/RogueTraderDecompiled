using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickMapObjectHandler : IClickEventHandler
{
	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		bool isOvertip = false;
		MapObjectView mapObj = gameObject?.GetComponentNonAlloc<MapObjectView>();
		return new HandlerPriorityResult((CheckMapObject(mapObj) && (CheckInteractions(mapObj, out isOvertip) || CheckDestructible(mapObj))) ? 1f : 0f, isOvertip);
	}

	private static bool CheckMapObject(MapObjectView mapObj)
	{
		if (mapObj == null)
		{
			return false;
		}
		if (!mapObj.Data.IsRevealed)
		{
			return false;
		}
		if (!mapObj.Data.IsAwarenessCheckPassed)
		{
			return false;
		}
		return true;
	}

	private bool CheckInteractions(MapObjectView mapObj, out bool isOvertip)
	{
		IEnumerable<InteractionPart> all = mapObj.Data.Parts.GetAll<InteractionPart>();
		bool flag = false;
		isOvertip = false;
		foreach (InteractionPart item in all)
		{
			if (!flag && item.CanInteract())
			{
				flag = true;
			}
			if (!isOvertip && item.Settings.ShowOvertip)
			{
				isOvertip = true;
			}
			if (flag & isOvertip)
			{
				break;
			}
		}
		if (flag)
		{
			return !isOvertip;
		}
		return false;
	}

	private bool CheckDestructible(MapObjectView mapObj)
	{
		if (mapObj is DestructibleEntityView)
		{
			return TurnController.IsInTurnBasedCombat();
		}
		return false;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		MapObjectView mapObject = ((gameObject != null) ? gameObject.GetComponent<MapObjectView>() : null);
		if (VariativeInteractionVM.HasVariativeInteraction(mapObject))
		{
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(mapObject);
			});
			return true;
		}
		List<BaseUnitEntity> units = Game.Instance.SelectionCharacter.SelectedUnits.ToList();
		return Interact(gameObject, units, forceOvertipInteractions: false, muteEvents);
	}

	public static bool HasAvailableInteractions(GameObject gameObject)
	{
		MapObjectView componentNonAlloc = gameObject.GetComponentNonAlloc<MapObjectView>();
		if (CheckMapObject(componentNonAlloc))
		{
			return componentNonAlloc.Data.Parts.GetAll<InteractionPart>().Any((InteractionPart interaction) => interaction.CanInteract());
		}
		return false;
	}

	public static bool Interact(GameObject gameObject, List<BaseUnitEntity> units, bool forceOvertipInteractions = false, bool muteEvents = false)
	{
		foreach (InteractionPart item in gameObject.GetComponent<MapObjectView>().Data.Parts.GetAll<InteractionPart>())
		{
			if ((!Game.Instance.IsControllerMouse || !item.Settings.ShowOvertip || forceOvertipInteractions) && TryInteract(item, units, muteEvents))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool TryInteract(InteractionPart interaction, List<BaseUnitEntity> users, bool muteEvents = false, IInteractionVariantActor variantActor = null)
	{
		BaseUnitEntity baseUnitEntity = interaction.SelectUnit(users, muteEvents);
		if (baseUnitEntity == null)
		{
			return false;
		}
		ShowWarningIfNeeded(baseUnitEntity, interaction);
		if (Game.Instance.TurnController.IsPreparationTurn)
		{
			return false;
		}
		if ((interaction.Type != 0 && interaction.Type != InteractionType.Approach) || !interaction.CanInteract())
		{
			return false;
		}
		if (interaction.Type == InteractionType.Direct && !muteEvents)
		{
			UnitCommandsRunner.DirectInteract(baseUnitEntity, interaction);
			return true;
		}
		bool leaveFollowers = false;
		if (interaction.HasVisibleTrap() || interaction is DisableTrapInteractionPart)
		{
			foreach (BaseUnitEntity user in users)
			{
				user.Commands.InterruptMove(byPlayer: true);
				(user.GetOptional<UnitPartPetOwner>()?.PetUnit)?.Commands.InterruptMove(byPlayer: true);
			}
			leaveFollowers = true;
		}
		if (interaction.Type == InteractionType.Approach)
		{
			UnitInteractWithObject.ApproachAndInteract(baseUnitEntity, interaction, variantActor, leaveFollowers);
		}
		return true;
	}

	public static void ShowWarningIfNeeded(BaseUnitEntity unit, InteractionPart interaction)
	{
		if (!TurnController.IsInTurnBasedCombat())
		{
			return;
		}
		string warning = null;
		ReasonStrings reasons = LocalizedTexts.Instance.Reasons;
		if (Game.Instance.TurnController.IsPreparationTurn)
		{
			warning = reasons.UnavailableGeneric;
		}
		if (interaction.AlreadyInteractedInThisCombatRound)
		{
			warning = reasons.AlreadyInteractedInThisCombatRound.Text;
		}
		if (unit != null && !interaction.HasEnoughActionPoints(unit))
		{
			warning = reasons.NotEnoughActionPoints.Text;
		}
		if (unit != null && !interaction.IsEnoughCloseForInteractionFromDesiredPosition(unit))
		{
			warning = reasons.InteractionIsTooFar.Text;
		}
		if (!string.IsNullOrEmpty(warning))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(warning, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
	}
}
