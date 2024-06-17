using System;
using Core.Cheats;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Net;
using Kingmaker.Utility.CircularBuffer;
using UnityEngine;

namespace Kingmaker.Controllers;

public class TimeSpeedController : IControllerTick, IController
{
	public const int MaxDelay = 8;

	private const int Seconds = 2;

	private const int StatLength = 40;

	private readonly CircularBuffer<byte> m_Lags = new CircularBuffer<byte>(40);

	private int m_LastTick = -1;

	private int m_CurrentSpeedMode;

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
		return TickType.Network;
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
