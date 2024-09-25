using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Replay;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

public class SynchronizedDataController : IControllerTick, IController, IControllerReset
{
	private readonly List<SynchronizedData> m_SynchronizedDataToSend = new List<SynchronizedData>(1) { default(SynchronizedData) };

	private readonly PlayerCommandsCollection<SynchronizedData> m_ReceivedSynchronizedData = new PlayerCommandsCollection<SynchronizedData>();

	private readonly List<(byte, CameraData)> m_LastReceivedMatrix = new List<(byte, CameraData)>(8);

	private readonly CommandQueue<SynchronizedData> m_DataQueue = new CommandQueue<SynchronizedData>();

	private (byte, Vector3, Quaternion, Vector4, Vector3, bool) m_LastLocalCamera;

	private bool m_HasLeftStickMovementData;

	private int m_LeftStickMovementDataFrame;

	private UnitReference m_Unit;

	private Vector2 m_MoveDirection;

	private float m_StickDeflection;

	private UnitReference[] m_SelectedUnits;

	public PlayerCommandsCollection<SynchronizedData> SynchronizedData => m_ReceivedSynchronizedData;

	public void PushDataForPlayer(NetPlayer player, int tickIndex, List<SynchronizedData> data)
	{
		m_DataQueue.PushCommandsForPlayer(player, tickIndex, data);
	}

	public void PrepareForSend(int sendTickIndex, List<SynchronizedData> unitCommands)
	{
		m_DataQueue.PrepareForSend(sendTickIndex, unitCommands);
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		FillData();
		m_DataQueue.SaveCommands(m_SynchronizedDataToSend, clear: false);
		while (m_DataQueue.LoadCommands(m_ReceivedSynchronizedData, onlyOneTick: true))
		{
			int index = NetworkingManager.LocalNetPlayer.Index;
			int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
			foreach (PlayerCommands<SynchronizedData> player in m_ReceivedSynchronizedData.Players)
			{
				int index2 = player.Player.Index;
				for (int i = 0; i < player.Commands.Count; i++)
				{
					SynchronizedData other = player.Commands[i];
					if (other.cameraType == 0)
					{
						continue;
					}
					m_LastReceivedMatrix.EnsureIndex(index2);
					byte cameraType = other.cameraType;
					CameraData camera = other.camera;
					switch (cameraType)
					{
					case 1:
					case 3:
					case 4:
						if (cameraType != 1)
						{
							CalculateMatrix(camera.position, camera.rotation, camera.projParams, out camera.matrix);
						}
						if (cameraType == m_LastReceivedMatrix[index2].Item1 && cameraType != 1 && m_LastReceivedMatrix[index2].Item2.IsEquals(camera))
						{
							PFLog.Net.Log(string.Format("[CAMERA] new and prev matrix are equal! t={0} p={1} {2} {3}", currentNetworkTick, index2, cameraType, camera?.matrix.GetHashCode().ToString("X") ?? "NULL"));
						}
						m_LastReceivedMatrix[index2] = (cameraType, camera);
						if (index2 == index)
						{
							m_LastReceivedMatrix[0] = (cameraType, camera);
						}
						break;
					case 2:
					{
						if (m_LastReceivedMatrix.TryGet(index2, out var element))
						{
							(cameraType, camera) = element;
							if ((camera == null && cameraType == 1) || (camera != null && (cameraType == 3 || cameraType == 4)))
							{
								player.Commands[i] = new SynchronizedData(other, cameraType, camera);
							}
							else
							{
								PFLog.Net.Error($"[CAMERA] Unexpected cameraType/cameraData combination '{cameraType}' '{camera != null}'!");
							}
						}
						else
						{
							PFLog.Net.Error($"[CAMERA] data not found! t={currentNetworkTick} p={index2}");
						}
						break;
					}
					default:
						PFLog.Net.Error($"[CAMERA] Unexpected cameraType value '{cameraType}'!");
						break;
					}
				}
			}
		}
		Kingmaker.Replay.Replay.SaveSynchronizedData(m_ReceivedSynchronizedData);
		Kingmaker.Replay.Replay.LoadSynchronizedData(m_ReceivedSynchronizedData);
	}

	[Conditional("REPLAY_LOG_ENABLED")]
	private void LogSynchronizedData()
	{
		foreach (PlayerCommands<SynchronizedData> player in m_ReceivedSynchronizedData.Players)
		{
			_ = player;
		}
	}

	void IControllerReset.OnReset()
	{
		Clear();
	}

	public void Clear()
	{
		m_LastReceivedMatrix.Clear();
		m_DataQueue.Reset();
		m_LastLocalCamera = default((byte, Vector3, Quaternion, Vector4, Vector3, bool));
		m_HasLeftStickMovementData = false;
		m_LeftStickMovementDataFrame = 0;
		m_Unit = default(UnitReference);
		m_MoveDirection = default(Vector2);
		m_StickDeflection = 0f;
		m_SelectedUnits = null;
	}

	public float GetMinScrollTimeBySpeed(Vector3 targetPos, float cameraSpeed, float maxTime = 0f, float maxSpeed = 0f)
	{
		float num = 60f;
		foreach (PlayerCommands<SynchronizedData> player in SynchronizedData.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				if (!command.IsEmpty)
				{
					num = Mathf.Min(CameraRig.Instance.CalculateScrollTime(command.camera.parentPosition, targetPos, maxTime, maxSpeed, cameraSpeed), num);
				}
			}
		}
		if (!(maxTime > 0f))
		{
			return num;
		}
		return Mathf.Min(num, maxTime);
	}

	public float GetMinRotateTimeBySpeed(float targetAngle, float cameraSpeed, float maxTime = 0f)
	{
		float num = 60f;
		foreach (PlayerCommands<SynchronizedData> player in SynchronizedData.Players)
		{
			foreach (SynchronizedData command in player.Commands)
			{
				if (!command.IsEmpty)
				{
					float sourceAngle = Mathf.Repeat(command.camera.rotation.eulerAngles.y + 180f, 360f);
					num = Mathf.Min(CameraRig.Instance.CalculateRotateTime(sourceAngle, targetAngle, maxTime, cameraSpeed), num);
				}
			}
		}
		if (!(maxTime > 0f))
		{
			return num;
		}
		return Mathf.Min(num, maxTime);
	}

	private void FillData()
	{
		SynchronizedData synchronizedData = default(SynchronizedData);
		synchronizedData.maxLag = Game.Instance.TimeSpeedController.MaxLag;
		synchronizedData.stateHash = Game.Instance.SyncStateCheckerController.StateHash;
		SynchronizedData data = synchronizedData;
		data = FillCameraData(data);
		data = FillLeftStickData(data);
		m_SynchronizedDataToSend[0] = data;
	}

	private static void CalculateMatrix(Vector3 camPos, Quaternion camRotate, Vector4 projParams, out Matrix4x4 cameraMatrix)
	{
		Matrix4x4 transpose = Matrix4x4.Rotate(camRotate).transpose;
		Vector3 vector = transpose * -camPos;
		transpose.SetColumn(3, new Vector4(vector.x, vector.y, vector.z, 1f));
		transpose.m20 = 0f - transpose.m20;
		transpose.m21 = 0f - transpose.m21;
		transpose.m22 = 0f - transpose.m22;
		transpose.m23 = 0f - transpose.m23;
		Matrix4x4 matrix4x = Matrix4x4.Perspective(projParams.x, projParams.y, projParams.z, projParams.w);
		cameraMatrix = matrix4x * transpose;
	}

	private SynchronizedData FillCameraData(SynchronizedData data)
	{
		Camera activeMainCamera = CameraStackManager.Instance.ActiveMainCamera;
		if (activeMainCamera == null)
		{
			m_LastLocalCamera = default((byte, Vector3, Quaternion, Vector4, Vector3, bool));
			return new SynchronizedData(data, 1);
		}
		bool num = CameraRig.Instance != null && CameraRig.Instance.Camera == activeMainCamera;
		Vector3 position = activeMainCamera.transform.position;
		Quaternion rotation = activeMainCamera.transform.rotation;
		Vector4 value = new Vector4(activeMainCamera.fieldOfView, activeMainCamera.aspect, activeMainCamera.nearClipPlane, activeMainCamera.farClipPlane);
		Vector3 value2 = (num ? CameraRig.Instance.transform.position : activeMainCamera.transform.parent.position);
		position = position.Round(6);
		rotation = rotation.Round(6);
		value = value.Round(6);
		value2 = value2.Round(6);
		byte b = (byte)(num ? 3 : 4);
		bool flag = num && CameraRig.Instance.IsScrollingByRoutine;
		(byte, Vector3, Quaternion, Vector4, Vector3, bool) tuple = (b, position, rotation, value, value2, flag);
		if (m_LastLocalCamera.Equals(tuple))
		{
			return new SynchronizedData(data, 2);
		}
		m_LastLocalCamera = tuple;
		CameraData camera = new CameraData
		{
			position = position,
			rotation = rotation,
			projParams = value,
			parentPosition = value2,
			isScrollingByRoutine = flag
		};
		return new SynchronizedData(data, b, camera);
	}

	private void CompressStickData(Vector2 moveDirection, float stickDeflection, out sbyte x, out sbyte y)
	{
		x = (sbyte)Mathf.Round(Mathf.Clamp(moveDirection.x * stickDeflection * 126f, -127f, 127f));
		y = (sbyte)Mathf.Round(Mathf.Clamp(moveDirection.y * stickDeflection * 126f, -127f, 127f));
	}

	public static void DecompressStickData(LeftStickData stickData, out Vector2 moveDirection, out float stickDeflection)
	{
		if (stickData.moveDirectionX == 0 && stickData.moveDirectionY == 0)
		{
			stickDeflection = 0f;
			moveDirection = Vector2.zero;
			return;
		}
		sbyte moveDirectionX = stickData.moveDirectionX;
		sbyte moveDirectionY = stickData.moveDirectionY;
		stickDeflection = Mathf.Sqrt(moveDirectionX * moveDirectionX + moveDirectionY * moveDirectionY);
		moveDirection.x = (float)Math.Round((float)moveDirectionX / stickDeflection, 2);
		moveDirection.y = (float)Math.Round((float)moveDirectionY / stickDeflection, 2);
		stickDeflection = (float)Math.Round(Mathf.Clamp01(stickDeflection / 126f), 2);
	}

	public void PushLeftStickMovement(BaseUnitEntity unit, Vector2 moveDirection, float stickDeflection)
	{
		m_HasLeftStickMovementData = true;
		m_LeftStickMovementDataFrame = 0;
		m_Unit = unit.FromBaseUnitEntity();
		m_MoveDirection = moveDirection;
		m_MoveDirection.x = (float)Math.Round(m_MoveDirection.x, 2);
		m_MoveDirection.y = (float)Math.Round(m_MoveDirection.y, 2);
		m_StickDeflection = (float)Math.Round(Mathf.Clamp01(stickDeflection), 2);
		m_SelectedUnits = GetSelectedUnits();
		static UnitReference[] GetSelectedUnits()
		{
			int count = Game.Instance.SelectionCharacter.SelectedUnits.Count;
			UnitReference[] array = new UnitReference[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = Game.Instance.SelectionCharacter.SelectedUnits[i].FromBaseUnitEntity();
			}
			return array;
		}
	}

	private SynchronizedData FillLeftStickData(SynchronizedData data)
	{
		if (m_HasLeftStickMovementData || m_LeftStickMovementDataFrame == Time.frameCount)
		{
			if (!GamepadInputController.CanProcessInput)
			{
				m_MoveDirection = Vector2.zero;
				m_StickDeflection = 0f;
			}
			CompressStickData(m_MoveDirection, m_StickDeflection, out var x, out var y);
			byte version = (byte)(Game.Instance.RealTimeController.CurrentNetworkTick % 255);
			data.leftStick = new LeftStickData
			{
				version = version,
				unit = m_Unit,
				moveDirectionX = x,
				moveDirectionY = y,
				selectedUnits = m_SelectedUnits
			};
			DecompressStickData(data.leftStick, out var moveDirection, out var stickDeflection);
			Game.Instance.MovePredictionController.PushLeftStickMovement(version, m_Unit, moveDirection, stickDeflection);
			m_HasLeftStickMovementData = false;
			m_LeftStickMovementDataFrame = Time.frameCount;
		}
		return data;
	}
}
