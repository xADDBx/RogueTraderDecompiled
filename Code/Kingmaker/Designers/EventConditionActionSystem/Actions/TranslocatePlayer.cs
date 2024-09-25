using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.QA;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/TranslocatePlayer")]
[AllowMultipleComponents]
[TypeId("6981071bc74236f4293f1a19ed2ebb32")]
public class TranslocatePlayer : GameAction
{
	[AllowedEntityType(typeof(LocatorView))]
	[Tooltip("Locator View")]
	[ValidateNotEmpty]
	public EntityReference transolcatePosition;

	public bool ByFormationAndWithPets;

	protected override void RunAction()
	{
		Vector3 position = transolcatePosition.FindView().ViewTransform.position;
		Quaternion rotation = transolcatePosition.FindView().ViewTransform.rotation;
		GameHelper.GetPlayerCharacter().View.StopMoving();
		if (AreaService.FindMechanicBoundsContainsPoint(position) != Game.Instance.CurrentlyLoadedAreaPart)
		{
			if (!Game.Instance.LoadedAreaState.Settings.CapitalPartyMode)
			{
				LogChannel.Default.ErrorWithReport($"{name}: cannot translocate, would change area parts. Use TeleportParty if you need to move to a different area part! (in {base.Owner})");
				return;
			}
			if (ByFormationAndWithPets)
			{
				PositionCharactersByFormation(position, rotation);
			}
			else
			{
				GameHelper.GetPlayerCharacter().Position = position;
			}
			CameraRig.Instance.ScrollToImmediately(position);
			CameraRig.Instance.UpdateForce();
			Game.Instance.MatchTimeOfDay();
		}
		else
		{
			if (ByFormationAndWithPets)
			{
				PositionCharactersByFormation(position, rotation);
			}
			else
			{
				GameHelper.GetPlayerCharacter().Position = position;
			}
			CameraRig.Instance.ScrollTo(position);
		}
	}

	public override string GetCaption()
	{
		return "Translocate Player";
	}

	private void PositionCharactersByFormation(Vector3 targetPosition, Quaternion targetRotation)
	{
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where(ShouldMoveCharacterOnLocator).ToList();
		Span<Vector3> resultPositions = stackalloc Vector3[list.Count];
		Vector3 direction = targetRotation * Vector3.forward;
		PartyFormationHelper.FillFormationPositions(targetPosition, FormationAnchor.Center, direction, list, list, Game.Instance.Player.FormationManager.CurrentFormation, resultPositions);
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].LifeState.IsFinallyDead)
			{
				Vector3 p = resultPositions[i];
				PositionCharacter(list[i], p, targetRotation);
			}
		}
	}

	private static void PositionCharacter(BaseUnitEntity character, Vector3 p, Quaternion rot)
	{
		character.Commands.InterruptAllInterruptible();
		ObjectExtensions.Or(character.View, null)?.StopMoving();
		character.Position = p;
		character.SetOrientation(rot.eulerAngles.y);
	}

	public bool ShouldMoveCharacterOnLocator(BaseUnitEntity character)
	{
		if (character != Game.Instance.Player.MainCharacterEntity)
		{
			if (ByFormationAndWithPets)
			{
				return character.Master == Game.Instance.Player.MainCharacterEntity;
			}
			return false;
		}
		return true;
	}
}
