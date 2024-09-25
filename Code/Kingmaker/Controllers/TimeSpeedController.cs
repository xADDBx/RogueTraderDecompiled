using System;
using Core.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.Networking;
using Kingmaker.Utility.CircularBuffer;
using UnityEngine;

namespace Kingmaker.Controllers;

public class TimeSpeedController : IControllerTick, IController
{
	public struct AutoSpeed
	{
		private static bool s_Enabled = true;

		private static float s_SlowValue = 0.99f;

		private const int AutoSpeedTargetDelay = 0;

		private int m_AutoSpeedCurrentDelay;

		[Cheat(Name = "net_auto_speed", ExecutionPolicy = ExecutionPolicy.PlayMode)]
		public static void AutoSpeedCheat(bool auto = false)
		{
			s_Enabled = auto;
		}

		[Cheat(Name = "net_speed_slow", ExecutionPolicy = ExecutionPolicy.PlayMode)]
		public static void SlowCheat(float value = 0.99f)
		{
			s_SlowValue = value;
		}

		public void Tick(TimeSpeedController timeSpeedController)
		{
			if (!s_Enabled)
			{
				return;
			}
			if (0 < m_AutoSpeedCurrentDelay)
			{
				m_AutoSpeedCurrentDelay--;
				return;
			}
			PlayerCommandsCollection<SynchronizedData> synchronizedData = Game.Instance.SynchronizedDataController.SynchronizedData;
			int num = int.MaxValue;
			int num2 = 0;
			int num3 = 0;
			foreach (PlayerCommands<SynchronizedData> player in synchronizedData.Players)
			{
				foreach (SynchronizedData command in player.Commands)
				{
					num = Mathf.Min(num, command.maxLag);
					num2 = Mathf.Max(num2, command.maxLag);
					if (player.Player.IsLocal)
					{
						num3 = command.maxLag;
					}
				}
			}
			if (num2 == num3 && num2 != num)
			{
				m_AutoSpeedCurrentDelay = 0;
				Game.Instance.RealTimeController.TimeScale = s_SlowValue;
			}
			else
			{
				Game.Instance.RealTimeController.TimeScale = 1f;
			}
		}
	}

	public const int MaxDelay = 8;

	private const int Seconds = 2;

	private const int StatLength = 40;

	private readonly CircularBuffer<byte> m_Lags = new CircularBuffer<byte>(40);

	private int m_LastTick = -1;

	private int m_CurrentSpeedMode;

	private AutoSpeed m_AutoSpeed;

	private static int m_ForceSpeedMode = -1;

	public byte MaxLag
	{
		get
		{
			byte b = 0;
			int i = 0;
			for (int count = m_Lags.Count; i < count; i++)
			{
				b = Math.Max(b, m_Lags[i]);
			}
			return b;
		}
	}

	public int CurrentSpeedMode => m_CurrentSpeedMode;

	public byte GetMinLag(byte defaultValue = 0)
	{
		int count = m_Lags.Count;
		if (count == 0)
		{
			return defaultValue;
		}
		byte b = byte.MaxValue;
		for (int i = 0; i < count; i++)
		{
			b = Math.Min(b, m_Lags[i]);
		}
		return b;
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		int maxLag = Game.Instance.SynchronizedDataController.SynchronizedData.GetMaxLag();
		if (m_CurrentSpeedMode < maxLag)
		{
			if (m_CurrentSpeedMode < 8)
			{
				m_CurrentSpeedMode++;
			}
		}
		else if (maxLag < m_CurrentSpeedMode && 0 < m_CurrentSpeedMode)
		{
			m_CurrentSpeedMode--;
		}
		if (m_ForceSpeedMode != -1)
		{
			m_CurrentSpeedMode = m_ForceSpeedMode;
		}
		m_AutoSpeed.Tick(this);
	}

	public int GetTickIndex(int tick)
	{
		return tick - m_CurrentSpeedMode;
	}

	public void Clear()
	{
		m_Lags.Clear();
		m_LastTick = -1;
		m_CurrentSpeedMode = 0;
	}

	public void OnEnoughTimeToProcess(int tick)
	{
		if (m_LastTick != tick)
		{
			m_LastTick = tick;
			int num = Game.Instance.RealTimeController.CurrentNetworkTick + 1;
			int maxReadyTick = Game.Instance.UnitCommandBuffer.GetMaxReadyTick();
			byte value = (byte)Mathf.Clamp(num - maxReadyTick, 0, 8);
			m_Lags.Append(value);
		}
	}

	[Cheat(Name = "net_speed_mode", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ForceSpeedMode(int speedMode = -1)
	{
		m_ForceSpeedMode = speedMode;
	}
}
