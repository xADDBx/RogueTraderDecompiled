using System;
using Code.Visual.Animation;
using Core.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CircularBuffer;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.MovePrediction;

public class MovePredictionController : IControllerTick, IController, IControllerDisable
{
	public struct Parameters
	{
		public float Acceleration;

		public float AngularSpeed;

		public float Corpulence;
	}

	public struct Data
	{
		public byte Version;

		public int Tick;

		public bool Predicted;

		public bool Simulated;

		public InputData Input;

		public OutputData Output;

		public void Predict(OutputData previousOutput, ref Parameters parameters, float deltaTime)
		{
			UnitMovementAgentContinuous unitMovementAgentContinuous = Input.Unit.ToBaseUnitEntity().View.MovementAgent as UnitMovementAgentContinuous;
			if (unitMovementAgentContinuous != null)
			{
				parameters.Acceleration = unitMovementAgentContinuous.m_Acceleration;
				parameters.AngularSpeed = unitMovementAgentContinuous.m_AngularSpeed;
				parameters.Corpulence = unitMovementAgentContinuous.Corpulence;
			}
			else
			{
				parameters.Acceleration = 20f;
				parameters.AngularSpeed = 360f;
				parameters.Corpulence = 0.5f * GraphParamsMechanicsCache.GridCellSize * 0.55f;
			}
			Output = previousOutput;
			TickMovementStatic(Input, ref Output, parameters, deltaTime);
			Predicted = true;
		}
	}

	public struct InputData
	{
		public UnitReference Unit;

		public Vector2 Direction;

		public float Deflection;
	}

	public struct OutputData
	{
		public Vector3 Position;

		public Vector2 Direction;

		public float Speed;

		public bool EnableSlidingAssist;

		public float CurrentSlidingAngle;

		public int SlidingAssistDirection;

		public CustomGridNodeBase CurrentNode;

		public float Time;

		public float StickDeflection;

		public bool Approximately(OutputData other)
		{
			if (Position.Approximately(other.Position) && Speed.Approximately(other.Speed) && EnableSlidingAssist == other.EnableSlidingAssist && CurrentSlidingAngle.Approximately(other.CurrentSlidingAngle))
			{
				return SlidingAssistDirection == other.SlidingAssistDirection;
			}
			return false;
		}

		public bool Equals(OutputData other)
		{
			if (Position == other.Position && Direction == other.Direction && Speed == other.Speed && EnableSlidingAssist == other.EnableSlidingAssist && CurrentSlidingAngle == other.CurrentSlidingAngle && SlidingAssistDirection == other.SlidingAssistDirection)
			{
				Int3? @int = CurrentNode?.position;
				Int3? int2 = other.CurrentNode?.position;
				if (@int.HasValue != int2.HasValue)
				{
					return false;
				}
				if (!@int.HasValue)
				{
					return true;
				}
				return @int.GetValueOrDefault() == int2.GetValueOrDefault();
			}
			return false;
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("MovePredictionController");

	private const int DelayInTicks = 1;

	private const int BufferLength = 16;

	private readonly CircularBuffer<Data> m_InputDataBuffer = new CircularBuffer<Data>(16);

	private Parameters m_LastParameters;

	private float m_LastDeltaTime;

	private static bool s_IsActive = true;

	public bool IsActive
	{
		get
		{
			if (s_IsActive)
			{
				return Game.Instance.IsControllerGamepad;
			}
			return false;
		}
	}

	public bool HasPrediction { get; private set; }

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		HasPrediction = false;
		if (!IsActive)
		{
			return;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			Reset();
			return;
		}
		m_LastDeltaTime = Game.Instance.TimeController.GameDeltaTime;
		foreach (PlayerCommands<SynchronizedData> player in Game.Instance.SynchronizedDataController.SynchronizedData.Players)
		{
			if (!player.Player.IsLocal)
			{
				continue;
			}
			foreach (SynchronizedData command in player.Commands)
			{
				MarkAsSimulated(command);
			}
		}
	}

	void IControllerDisable.OnDisable()
	{
		Reset();
	}

	private void Reset()
	{
		m_InputDataBuffer.Clear();
		HasPrediction = false;
	}

	public void PushLeftStickMovement(byte version, UnitReference unitRef, Vector2 moveDirection, float stickDeflection)
	{
		int tick = Game.Instance.RealTimeController.CurrentNetworkTick + 1;
		Data data = default(Data);
		data.Version = version;
		data.Tick = tick;
		data.Input = new InputData
		{
			Unit = unitRef,
			Direction = moveDirection,
			Deflection = stickDeflection
		};
		Data value = data;
		m_InputDataBuffer.Append(value);
	}

	public bool GetData(out BaseUnitEntity unit, out OutputData oldOutput, out OutputData newOutput)
	{
		if (!Game.Instance.IsControllerGamepad)
		{
			unit = null;
			oldOutput = default(OutputData);
			newOutput = default(OutputData);
			return false;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < m_InputDataBuffer.Count; i++)
		{
			if (m_InputDataBuffer[i].Tick < currentNetworkTick)
			{
				num2 = i;
			}
			else if (m_InputDataBuffer[i].Tick == currentNetworkTick)
			{
				num = i;
			}
		}
		bool flag = num != -1;
		if (!flag)
		{
			if (num2 == -1)
			{
				unit = null;
				oldOutput = default(OutputData);
				newOutput = default(OutputData);
				return false;
			}
			num = num2;
		}
		Data value = m_InputDataBuffer[num];
		if (value.Simulated)
		{
			unit = null;
			oldOutput = default(OutputData);
			newOutput = default(OutputData);
			return false;
		}
		unit = value.Input.Unit.ToBaseUnitEntity();
		oldOutput = new OutputData
		{
			Position = unit.Position,
			Direction = ((unit.MovementAgent != null && unit.MovementAgent.IsReallyMoving) ? unit.MovementAgent.MoveDirection : unit.OrientationDirection.To2D())
		};
		int num3 = 0;
		for (int j = 0; j < num; j++)
		{
			if (m_InputDataBuffer[j].Input.Unit != unit)
			{
				num3 = j + 1;
			}
		}
		for (int k = num3; k < num; k++)
		{
			if (m_InputDataBuffer[k].Predicted)
			{
				oldOutput = m_InputDataBuffer[k].Output;
				continue;
			}
			Data value2 = m_InputDataBuffer[k];
			value2.Predict(oldOutput, ref m_LastParameters, m_LastDeltaTime);
			oldOutput = value2.Output;
			m_InputDataBuffer[k] = value2;
		}
		if (!value.Predicted)
		{
			value.Predict(oldOutput, ref m_LastParameters, m_LastDeltaTime);
			m_InputDataBuffer[num] = value;
		}
		newOutput = value.Output;
		if (!flag)
		{
			oldOutput = newOutput;
		}
		HasPrediction = oldOutput.Position != newOutput.Position;
		return true;
	}

	private void MarkAsSimulated(SynchronizedData synchronizedData)
	{
		if (synchronizedData.leftStick == null)
		{
			return;
		}
		byte version = synchronizedData.leftStick.version;
		version = (byte)((255 + version) % 255);
		int num = -1;
		for (int i = 0; i < m_InputDataBuffer.Count; i++)
		{
			Data data = m_InputDataBuffer[i];
			if (version == data.Version)
			{
				num = i;
				break;
			}
			if ((version < data.Version && data.Version - version < 127) || (version < data.Version + 255 + 1 && data.Version + 255 + 1 - version < 127))
			{
				num = i - 1;
				break;
			}
		}
		if (num == -1)
		{
			Logger.Log($"MarkAsSimulated ver={version} not found!");
			return;
		}
		for (int j = 0; j <= num; j++)
		{
			Data value = m_InputDataBuffer[j];
			value.Simulated = true;
			m_InputDataBuffer[j] = value;
		}
		Data value2 = m_InputDataBuffer[num];
		BaseUnitEntity baseUnitEntity = value2.Input.Unit.ToBaseUnitEntity();
		UnitMovementAgentContinuous unitMovementAgentContinuous = baseUnitEntity.MovementAgent as UnitMovementAgentContinuous;
		OutputData outputData = default(OutputData);
		outputData.Position = baseUnitEntity.Position;
		outputData.Direction = ((baseUnitEntity.MovementAgent != null && baseUnitEntity.MovementAgent.IsReallyMoving) ? baseUnitEntity.MovementAgent.MoveDirection : baseUnitEntity.OrientationDirection.To2D());
		outputData.Speed = ((baseUnitEntity.MovementAgent != null) ? baseUnitEntity.MovementAgent.Speed : 0f);
		outputData.EnableSlidingAssist = unitMovementAgentContinuous != null && unitMovementAgentContinuous.EnableSlidingAssist;
		outputData.CurrentSlidingAngle = ((unitMovementAgentContinuous != null) ? unitMovementAgentContinuous.CurrentSlidingAngle : 0f);
		outputData.SlidingAssistDirection = ((unitMovementAgentContinuous != null) ? unitMovementAgentContinuous.SlidingAssistDirection : 0);
		outputData.CurrentNode = ((unitMovementAgentContinuous != null) ? unitMovementAgentContinuous.CurrentNode : null);
		outputData.Time = value2.Output.Time;
		outputData.StickDeflection = value2.Output.StickDeflection;
		OutputData outputData2 = outputData;
		if (value2.Predicted && !value2.Output.Equals(outputData2))
		{
			value2.Output.Approximately(outputData2);
			value2.Output = outputData2;
			m_InputDataBuffer[num] = value2;
		}
		else
		{
			value2.Predicted = true;
			value2.Output = outputData2;
			m_InputDataBuffer[num] = value2;
		}
		for (int k = num + 1; k < m_InputDataBuffer.Count; k++)
		{
			Data value3 = m_InputDataBuffer[k];
			if (value3.Predicted)
			{
				value3.Predicted = false;
				m_InputDataBuffer[k] = value3;
				continue;
			}
			break;
		}
	}

	[Cheat(Name = "net_move", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetActive(bool value)
	{
		s_IsActive = value;
	}

	private static void TickMovementStatic(InputData inputData, ref OutputData outputData, Parameters parameters, float deltaTime)
	{
		if (Game.Instance.CurrentMode != GameModeType.Default)
		{
			return;
		}
		BaseUnitEntity baseUnitEntity = inputData.Unit.ToBaseUnitEntity();
		UnitEntityView view = baseUnitEntity.View;
		UnitAnimationManager unitAnimationManager = view.AnimationManager.Or(null);
		if (((object)unitAnimationManager != null && unitAnimationManager.IsPreventingMovement) || (view.MovementAgent.Or(null)?.NodeLinkTraverser?.IsTraverseNow).GetValueOrDefault() || view.IsCommandsPreventMovement)
		{
			outputData.Speed = 0f;
			outputData.Time = 0f;
			outputData.StickDeflection = 0f;
			return;
		}
		using (ProfileScope.New("Tick Movement Continous"))
		{
			if (inputData.Deflection < 0.0001f)
			{
				outputData.Speed = 0f;
				outputData.Time = 0f;
				outputData.StickDeflection = 0f;
				return;
			}
			float maxSpeed = GetMaxSpeed(baseUnitEntity, inputData.Deflection, outputData.Time);
			maxSpeed = UnitMovementAgentContinuous.GetSpeedByControllerStickDeflection(inputData.Deflection, maxSpeed);
			Vector2 direction = inputData.Direction;
			Vector2 vector = direction;
			float speed = ((Mathf.Abs(maxSpeed - outputData.Speed) < parameters.Acceleration * deltaTime) ? maxSpeed : (outputData.Speed + Mathf.Sign(maxSpeed - outputData.Speed) * parameters.Acceleration * deltaTime));
			if (outputData.EnableSlidingAssist)
			{
				UnitMovementAgentContinuous.UpdateSliding(outputData.Position, vector, deltaTime, ref outputData.SlidingAssistDirection, ref outputData.CurrentSlidingAngle);
				vector = vector.RotateAroundPoint(Vector2.zero, outputData.CurrentSlidingAngle);
			}
			vector = Vector2.Lerp(direction, vector, 0.5f);
			float angularSpeed = parameters.AngularSpeed;
			direction = Vector3.RotateTowards(direction, vector, angularSpeed * deltaTime * (MathF.PI / 180f), 1f);
			outputData.Speed = speed;
			Vector3 vector2 = direction.To3D() * outputData.Speed;
			CustomGridNodeBase targetNode;
			Vector3 vector3 = UnitMovementAgentBase.Move(outputData.Position, vector2 * deltaTime, parameters.Corpulence, out targetNode);
			Vector3 vector4 = vector3 - (outputData.Position + vector2 * deltaTime);
			if (Mathf.Abs(vector4.x) < 0.01f && Mathf.Abs(vector4.z) < 0.01f)
			{
				outputData.EnableSlidingAssist = false;
				outputData.CurrentSlidingAngle = 0f;
				outputData.SlidingAssistDirection = 0;
			}
			else
			{
				outputData.EnableSlidingAssist = true;
			}
			if (!NodeLinksExtensions.AreConnected(outputData.CurrentNode, targetNode, out var _))
			{
				outputData.Position = vector3;
				outputData.CurrentNode = targetNode;
			}
			outputData.Direction = direction;
			bool flag = GetMovementType(outputData.StickDeflection) != GetMovementType(inputData.Deflection) && GetMovementType(inputData.Deflection) != WalkSpeedType.Sprint;
			flag = false;
			outputData.Time = (flag ? 0f : Mathf.Min(outputData.Time + deltaTime, 9f));
			outputData.StickDeflection = inputData.Deflection;
		}
		static float GetMaxSpeed(BaseUnitEntity unit, float stickDeflection, float time)
		{
			UnitAnimationManager maybeAnimationManager = unit.MaybeAnimationManager;
			if (maybeAnimationManager == null)
			{
				return 0f;
			}
			WarhammerUnitAnimationActionLocoMotionHuman warhammerUnitAnimationActionLocoMotionHuman = maybeAnimationManager.GetAction(UnitAnimationType.LocoMotion) as WarhammerUnitAnimationActionLocoMotionHuman;
			if (warhammerUnitAnimationActionLocoMotionHuman == null)
			{
				return 0f;
			}
			WarhammerUnitAnimationActionLocoMotionHuman.MovementStyleLayer nonCombatWalk = warhammerUnitAnimationActionLocoMotionHuman.NonCombatWalk;
			WarhammerUnitAnimationActionLocoMotionHuman.CurrentWalkingStyleLayer currentWalkingStyleLayer = GetMovementType(stickDeflection) switch
			{
				WalkSpeedType.Sprint => nonCombatWalk.Sprint, 
				WalkSpeedType.Run => nonCombatWalk.Run, 
				_ => nonCombatWalk.Walking, 
			};
			float b = ((!(0f < time)) ? 0f : ((currentWalkingStyleLayer.In != null && time < currentWalkingStyleLayer.In.Length) ? currentWalkingStyleLayer.InSpeed : currentWalkingStyleLayer.Speed));
			return Mathf.Max(0.01f, b);
		}
		static WalkSpeedType GetMovementType(float multiplier)
		{
			if (multiplier > 0.95f)
			{
				return WalkSpeedType.Sprint;
			}
			if (multiplier > 0.7f)
			{
				return WalkSpeedType.Run;
			}
			return WalkSpeedType.Walk;
		}
	}
}
