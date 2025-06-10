using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Formations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[TypeId("c13acfdc96afb6641b748bc39e972c97")]
public class CommandMoveParty : CommandBase
{
	private class Data
	{
		public List<UnitData> AffectedUnits = new List<UnitData>();
	}

	public class UnitData
	{
		public EntityRef<BaseUnitEntity> Unit;

		public Path Path;

		public UnitCommandHandle Command;

		public Vector3 TargetPosition;

		public Quaternion TargetRotation;

		public bool Interrupt;
	}

	private static readonly LogChannel Logger = PFLog.Cutscene;

	[SerializeField]
	private Player.CharactersList m_UnitsList;

	[SerializeReference]
	public AbstractUnitEvaluator[] ExceptThese;

	[HideIf("True")]
	public EntityReference[] Targets;

	[ValidateNotEmpty]
	[ValidateNoNullEntries]
	public CommandTranslocateParty.Target[] TargetsV2;

	public WalkSpeedType Animation = WalkSpeedType.Walk;

	public bool OverrideSpeed;

	[ConditionalShow("OverrideSpeed")]
	public float Speed = 5f;

	public bool DisableAvoidance = true;

	public bool MoveWithFormation;

	[ShowIf("MoveWithFormation")]
	public float FormationSpaceFactor = 1f;

	private readonly double m_Timeout = 20.0;

	private bool True => true;

	public override void OnEnable()
	{
		base.OnEnable();
		if (TargetsV2.Empty() && !Targets.Empty())
		{
			TargetsV2 = Targets?.Select((EntityReference x) => new CommandTranslocateParty.Target
			{
				Entity = x
			}).ToArray();
			Targets = null;
		}
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		int num = 0;
		AbstractUnitEntity value;
		IEnumerable<BaseUnitEntity> enumerable = from x in Game.Instance.Player.GetCharactersList(m_UnitsList)
			where !x.HasMechanicFeature(MechanicsFeatureType.Hidden) && x.IsInGame
			where !ElementExtendAsObject.Valid(ExceptThese).Any((AbstractUnitEvaluator y) => y != null && y.TryGetValue(out value) && x == value)
			select x;
		if (MoveWithFormation)
		{
			Vector3[] formationPositions = GetFormationPositions();
			{
				foreach (BaseUnitEntity item in enumerable)
				{
					Vector3 targetPosition2 = formationPositions[num++ % formationPositions.Length];
					Move(item, targetPosition2);
				}
				return;
			}
		}
		CommandTranslocateParty.Distribute(enumerable, TargetsV2, delegate(BaseUnitEntity character, EntityReference targetRef, int characterIndex)
		{
			IEntityViewBase entityViewBase = targetRef.FindView();
			if (entityViewBase == null)
			{
				Logger.Error($"{this}: Can't find locator {targetRef} for teleport unit");
			}
			else
			{
				Move(character, entityViewBase.ViewTransform.position, entityViewBase.ViewTransform.rotation);
			}
		});
		UnitData Move(BaseUnitEntity character, Vector3 targetPosition, Quaternion? targetRotation = null)
		{
			UnitData affectedUnit = new UnitData
			{
				Unit = character,
				TargetPosition = targetPosition,
				Interrupt = skipping
			};
			if (targetRotation.HasValue)
			{
				affectedUnit.TargetRotation = targetRotation.Value;
			}
			commandData.AffectedUnits.Add(affectedUnit);
			if (!skipping)
			{
				affectedUnit.Path = PathfindingService.Instance.FindPathRT_Delayed(character.MovementAgent, affectedUnit.TargetPosition, 0.3f, 1, delegate(ForcedPath path)
				{
					affectedUnit.Path = null;
					if (path.error)
					{
						PFLog.Pathfinding.Error("An error path was returned. Ignoring");
					}
					else if (!affectedUnit.Interrupt)
					{
						UnitMoveToParams cmdParams = new UnitMoveToParams(path, affectedUnit.TargetPosition)
						{
							OverrideSpeed = (OverrideSpeed ? new float?(Speed) : null),
							MovementType = Animation,
							Orientation = targetRotation?.eulerAngles.y
						};
						BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
						if (baseUnitEntity == null)
						{
							CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
						}
						else
						{
							UnitCommandHandle command = baseUnitEntity.Commands.Run(cmdParams);
							affectedUnit.Command = command;
							if (DisableAvoidance)
							{
								baseUnitEntity.View.MovementAgent.AvoidanceDisabled = true;
							}
						}
					}
				});
			}
			return affectedUnit;
		}
	}

	private Vector3[] GetFormationPositions()
	{
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable).ToList();
		IPartyFormation currentFormation = Game.Instance.Player.FormationManager.CurrentFormation;
		Span<Vector3> resultPositions = stackalloc Vector3[list.Count];
		Vector3 position = (Targets[0].FindData() ?? throw new Exception("No data for target at " + name)).Position;
		PartyFormationHelper.FillFormationPositions(position, FormationAnchor.Front, ClickGroundHandler.GetDirection(position, list), list, list, currentFormation, resultPositions);
		return resultPositions.ToArray();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		foreach (UnitData affectedUnit in player.GetCommandData<Data>(this).AffectedUnits)
		{
			if (affectedUnit.Path != null)
			{
				return false;
			}
			UnitCommandHandle command = affectedUnit.Command;
			if (command != null && !command.IsFinished)
			{
				return false;
			}
		}
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		if (time > m_Timeout)
		{
			InterruptImpl(commandData);
		}
	}

	public override void Interrupt(CutscenePlayerData player)
	{
		base.Interrupt(player);
		Data commandData = player.GetCommandData<Data>(this);
		InterruptImpl(commandData);
	}

	private void InterruptImpl(Data commandData)
	{
		foreach (UnitData affectedUnit in commandData.AffectedUnits)
		{
			affectedUnit.Interrupt = true;
			affectedUnit.Path = null;
			UnitCommandHandle command = affectedUnit.Command;
			if (command != null && !command.IsFinished)
			{
				command.Interrupt();
			}
		}
	}

	protected override void OnStop(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		TeleportEveryoneFailedToArrive(commandData);
		if (DisableAvoidance)
		{
			foreach (UnitData affectedUnit in commandData.AffectedUnits)
			{
				BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
				if (baseUnitEntity == null)
				{
					CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
				}
				else
				{
					baseUnitEntity.View.MovementAgent.AvoidanceDisabled = false;
				}
			}
		}
		foreach (UnitData affectedUnit2 in commandData.AffectedUnits)
		{
			((BaseUnitEntity)affectedUnit2.Unit).SetOrientation(affectedUnit2.TargetRotation.eulerAngles.y);
		}
	}

	public override string GetCaption()
	{
		return $"Move party ({m_UnitsList})";
	}

	private void TeleportEveryoneFailedToArrive(Data commandData)
	{
		foreach (UnitData affectedUnit in commandData.AffectedUnits)
		{
			UnitCommandHandle command = affectedUnit.Command;
			if (command == null || command.Result != AbstractUnitCommand.ResultType.Success)
			{
				BaseUnitEntity baseUnitEntity = affectedUnit.Unit;
				if (baseUnitEntity == null)
				{
					CutscenePlayerData.Logger.Error("Lost unit {0} while executing {1}", affectedUnit.Unit, this);
				}
				else
				{
					baseUnitEntity.Translocate(affectedUnit.TargetPosition, null);
				}
			}
		}
	}
}
