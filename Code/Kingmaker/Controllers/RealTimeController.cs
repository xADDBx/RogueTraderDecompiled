using System;
using Kingmaker.Networking;
using Kingmaker.QA.Overlays;
using Kingmaker.Replay;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers;

public class RealTimeController
{
	private static TimeSpan MaxDeltaTime = (BuildModeUtility.Data.LimitDeltaTimeForProfiling ? TimeSpan.FromMilliseconds(60.0) : TimeSpan.FromMilliseconds(250.0));

	public float TimeScale = 1f;

	private int m_TickAtFrame = -1;

	private int m_LastProcessedFrame = -1;

	private TimeSpan m_DeltaTimeInterpolation = TimeSpan.Zero;

	private TimeSpan m_DeltaTimeToProcess = TimeSpan.Zero;

	private bool m_IsEndFrameTickProcessing;

	private bool m_TickStarted;

	private bool m_ForceSystemTick;

	public bool IsSimulationTick;

	public const long NetworkStepMs = 50L;

	private TimeSpan m_InterpolationTime;

	public static TimeSpan SystemStepTimeSpan => TimeSpan.FromTicks(500000L);

	public static float SystemStepDurationSeconds => (float)SystemStepTimeSpan.TotalSeconds;

	public int CurrentSystemStepIndex => GetSystemStepIndex(Game.Instance.Player.RealTime);

	public static TimeSpan NetworkStepTimeSpan => TimeSpan.FromTicks(500000L);

	public TimeSpan NetworkDeltaTimeSpan
	{
		get
		{
			if (!IsSimulationTick)
			{
				return TimeSpan.Zero;
			}
			return NetworkStepTimeSpan;
		}
	}

	public float NetworkDeltaTime => (float)NetworkDeltaTimeSpan.TotalSeconds;

	public int CurrentNetworkTick
	{
		get
		{
			if (Game.Instance.Player == null)
			{
				return 0;
			}
			return GetNetworkTick(Game.Instance.Player.RealTime);
		}
	}

	public bool OneMoreTick
	{
		get
		{
			if (!IsStepAvailable(out var _))
			{
				return !m_IsEndFrameTickProcessing;
			}
			return true;
		}
	}

	public bool IsBeginFrameTick => m_TickAtFrame == 0;

	public bool IsEndFrameTick => m_IsEndFrameTickProcessing;

	public float SystemDeltaTime => (float)SystemDeltaTimeSpan.TotalSeconds;

	public TimeSpan SystemDeltaTimeSpan
	{
		get
		{
			if (!IsSimulationTick)
			{
				return TimeSpan.Zero;
			}
			return SystemStepTimeSpan;
		}
	}

	public TimeSpan DeltaTimeInterpolation => m_InterpolationTime;

	public float InterpolationProgress => (float)(m_InterpolationTime / SystemStepTimeSpan);

	public bool TickCompleted => !m_TickStarted;

	private int GetSystemStepIndex(TimeSpan timeSpan)
	{
		return (int)(timeSpan.Ticks / SystemStepTimeSpan.Ticks);
	}

	private int GetNetworkTick(TimeSpan timeSpan)
	{
		return (int)(timeSpan.Ticks / NetworkStepTimeSpan.Ticks);
	}

	private bool IsStepAvailable(out bool commandsReady)
	{
		commandsReady = true;
		Player player = Game.Instance.Player;
		if (player == null)
		{
			return false;
		}
		TimeSpan realTime = player.RealTime;
		TimeSpan timeSpan = realTime;
		realTime += m_DeltaTimeToProcess;
		int systemStepIndex = GetSystemStepIndex(timeSpan);
		int systemStepIndex2 = GetSystemStepIndex(realTime);
		if (systemStepIndex == systemStepIndex2)
		{
			return false;
		}
		int networkTick = GetNetworkTick(timeSpan);
		int networkTick2 = GetNetworkTick(realTime);
		if (networkTick != networkTick2)
		{
			commandsReady = NextTickCommandsReady(networkTick);
			return commandsReady;
		}
		return true;
	}

	public bool NextTickCommandsReady(int currentTickIndex)
	{
		int tickIndex = Game.Instance.TimeSpeedController.GetTickIndex(currentTickIndex + 1);
		return NetLagTest.Process(true && Game.Instance.GameCommandQueue.IsReady(tickIndex), tickIndex);
	}

	public void Tick()
	{
		if (m_TickStarted)
		{
			return;
		}
		m_TickAtFrame++;
		int frameCount = Time.frameCount;
		bool flag = m_LastProcessedFrame != frameCount;
		if (flag)
		{
			m_LastProcessedFrame = frameCount;
			TimeSpan deltaTime = (m_ForceSystemTick ? MaxDeltaTime : Time.unscaledDeltaTime.Seconds());
			deltaTime *= (double)TimeScale;
			deltaTime = NetLagTest.Process(deltaTime);
			TimeSpan deltaTimeToProcess = m_DeltaTimeToProcess;
			m_DeltaTimeToProcess += deltaTime;
			if (!m_ForceSystemTick)
			{
				m_DeltaTimeToProcess = NetworkingManager.ProcessDeltaTime(Game.Instance.Player.RealTime, m_DeltaTimeToProcess);
			}
			m_DeltaTimeToProcess = TimeSpan.FromTicks(Math.Min(m_DeltaTimeToProcess.Ticks, MaxDeltaTime.Ticks));
			m_DeltaTimeInterpolation += m_DeltaTimeToProcess - deltaTimeToProcess;
			m_InterpolationTime = new TimeSpan(Math.Min(m_DeltaTimeInterpolation.Ticks, 2 * SystemStepTimeSpan.Ticks));
			m_TickAtFrame = 0;
			if (SystemStepTimeSpan <= m_DeltaTimeToProcess)
			{
				Game.Instance.TimeSpeedController.OnEnoughTimeToProcess(CurrentNetworkTick);
			}
		}
		TimeSpan realTime = Game.Instance.Player.RealTime;
		if (IsStepAvailable(out var commandsReady))
		{
			long num = GetSystemStepIndex(realTime);
			num++;
			TimeSpan realTime2 = SystemStepTimeSpan * num;
			TimeSpan systemStepTimeSpan = SystemStepTimeSpan;
			Game.Instance.Player.RealTime = realTime2;
			m_DeltaTimeToProcess -= systemStepTimeSpan;
			m_DeltaTimeInterpolation = new TimeSpan(m_DeltaTimeToProcess.Ticks % SystemStepTimeSpan.Ticks);
			m_InterpolationTime = m_DeltaTimeInterpolation;
			IsSimulationTick = true;
			m_IsEndFrameTickProcessing = false;
			NetworkingOverlay.NewTick();
		}
		else
		{
			if (flag && !commandsReady)
			{
				NetworkingOverlay.AddSkipTick();
			}
			IsSimulationTick = false;
			m_IsEndFrameTickProcessing = 0 < m_TickAtFrame;
		}
		Kingmaker.Replay.Replay.SaveState();
		m_ForceSystemTick = false;
		m_TickStarted = true;
	}

	public void FinishTick()
	{
		m_TickStarted = false;
	}

	public void Suspend()
	{
		m_DeltaTimeToProcess = TimeSpan.Zero;
		m_IsEndFrameTickProcessing = true;
		m_ForceSystemTick = true;
	}

	public int SystemStepIndexAfter(TimeSpan value)
	{
		return CurrentSystemStepIndex + (int)Math.Ceiling(value.TotalSeconds / (double)SystemStepDurationSeconds);
	}
}
