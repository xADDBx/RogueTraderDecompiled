using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers;

public class GamepadInputController : IControllerTick, IController, IControllerReset
{
	public struct PlayerData
	{
		private float m_RepathTimer;

		private float m_RepathTimerLimit;

		public void Tick(NetPlayer player, LeftStickData leftStickData)
		{
			if (leftStickData == null)
			{
				return;
			}
			BaseUnitEntity unit = leftStickData.unit.ToBaseUnitEntity();
			if (unit == null)
			{
				return;
			}
			SynchronizedDataController.DecompressStickData(leftStickData, out var moveDirection, out var stickDeflection);
			UnitMovementAgentBase unityObject = unit.View.Or(null)?.MovementAgent;
			UnitMovementAgentBase unitMovementAgentBase = unityObject.Or(null);
			if ((object)unitMovementAgentBase != null && unitMovementAgentBase.IsTraverseInProgress)
			{
				return;
			}
			bool flag = unityObject.Or(null)?.IsReallyMoving ?? false;
			flag &= unit.View.Or(null)?.AgentOverride as UnitMovementAgentContinuous != null;
			if (flag || moveDirection.sqrMagnitude > Mathf.Epsilon)
			{
				UnitMoveContinuouslyParams unitMoveContinuouslyParams = new UnitMoveContinuouslyParams(moveDirection, stickDeflection)
				{
					MovementType = WalkSpeedType.Walk
				};
				if (unit.Commands.CurrentMoveContinuously != null)
				{
					unitMoveContinuouslyParams.TryMergeInto(unit.Commands.CurrentMoveContinuously);
				}
				else
				{
					unit.Commands.RunImmediate(unitMoveContinuouslyParams);
				}
			}
			if (moveDirection.sqrMagnitude < Mathf.Epsilon)
			{
				if (flag)
				{
					MakeCompanionsFollow(player, unit, leftStickData.selectedUnits);
				}
				return;
			}
			if (unit.Commands.CurrentMoveContinuously == null)
			{
				m_RepathTimer = 0f;
				m_RepathTimerLimit = 0.1f;
				unit.Commands.InterruptMove();
			}
			if (player.IsLocal && Game.Instance.CameraController?.Follower != null)
			{
				Game.Instance.CameraController.Follower.Follow(unit);
			}
			if (m_RepathTimer > m_RepathTimerLimit)
			{
				m_RepathTimer = 0f;
				m_RepathTimerLimit = Game.Instance.Player.Party.Min((BaseUnitEntity u) => (u != unit) ? u.View.MovementAgent.EstimatedTimeLeft : float.MaxValue);
				m_RepathTimerLimit = Mathf.Clamp(m_RepathTimerLimit, 0.1f, 0.5f);
				MakeCompanionsFollow(player, unit, leftStickData.selectedUnits);
			}
			else
			{
				m_RepathTimer += Game.Instance.RealTimeController.SystemDeltaTime;
			}
		}

		private static void MakeCompanionsFollow(NetPlayer player, BaseUnitEntity unit, UnitReference[] selectedUnits)
		{
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.IncreaseCapacity(selectedUnits.Length);
			for (int i = 0; i < selectedUnits.Length; i++)
			{
				UnitReference r = selectedUnits[i];
				if ((r.Entity?.ToBaseUnitEntity()?.IsDirectlyControllable(player)).GetValueOrDefault())
				{
					list.Add(r.ToBaseUnitEntity());
				}
			}
			List<BaseUnitEntity> allUnits = ((list.Count == 1) ? list : Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable(player)).ToList());
			UnitCommandsRunner.MoveSelectedUnitsToPointRT(unit, unit.Position, unit.Forward, isControllerGamepad: true, anchorOnMainUnit: true, preview: false, 1f, list, null, allUnits);
		}
	}

	private const int PlayerDataSize = 8;

	private readonly PlayerData[] m_PlayerData = new PlayerData[8];

	public static bool CanProcessInput
	{
		get
		{
			if (Game.Instance.CurrentMode == GameModeType.Default && !CutsceneLock.Active)
			{
				return !Game.Instance.Player.IsInCombat;
			}
			return false;
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (!CanProcessInput)
		{
			ForceStopUnits();
			return;
		}
		using (ContextData<UnitCommandContext>.Request())
		{
			foreach (PlayerCommands<SynchronizedData> player in Game.Instance.SynchronizedDataController.SynchronizedData.Players)
			{
				if (player.Commands.TryGet(0, out var element))
				{
					int index = player.Player.Index;
					m_PlayerData[index].Tick(player.Player, element.leftStick);
				}
			}
		}
	}

	void IControllerReset.OnReset()
	{
		Array.Fill(m_PlayerData, default(PlayerData));
	}

	private static void ForceStopUnits()
	{
		foreach (UnitReference partyCharacter in Game.Instance.Player.PartyCharacters)
		{
			if (!(partyCharacter.Entity is BaseUnitEntity baseUnitEntity))
			{
				continue;
			}
			UnitMovementAgentContinuous unitMovementAgentContinuous = baseUnitEntity.View.Or(null)?.MovementAgent as UnitMovementAgentContinuous;
			if (unitMovementAgentContinuous == null || !unitMovementAgentContinuous.IsReallyMoving)
			{
				continue;
			}
			UnitMoveContinuously currentMoveContinuously = baseUnitEntity.Commands.CurrentMoveContinuously;
			if (currentMoveContinuously == null || !currentMoveContinuously.Params.IsZero)
			{
				UnitMoveContinuouslyParams unitMoveContinuouslyParams = new UnitMoveContinuouslyParams(Vector2.zero, 0f)
				{
					MovementType = WalkSpeedType.Walk
				};
				if (currentMoveContinuously != null)
				{
					unitMoveContinuouslyParams.TryMergeInto(baseUnitEntity.Commands.CurrentMoveContinuously);
				}
				else
				{
					baseUnitEntity.Commands.RunImmediate(unitMoveContinuouslyParams);
				}
			}
		}
	}
}
