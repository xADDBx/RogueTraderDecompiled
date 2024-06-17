using System;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Cheats.Logic;

public class LogsSpammer : MonoBehaviour
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Spammer");

	private int m_Count;

	private TimeSpan m_Interval;

	private int m_Depth;

	private DateTime m_LastTime;

	private SpamType m_Type;

	private static int TotalData = 0;

	private static readonly System.Random Random = new System.Random();

	public LogsSpammer()
	{
		TotalData = 0;
	}

	private void Update()
	{
		if (m_Count == 0)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		TimeSpan timeSpan = DateTime.Now - m_LastTime;
		while (timeSpan > m_Interval)
		{
			timeSpan -= m_Interval;
			try
			{
				DebugSpam(m_Type, m_Depth);
			}
			catch (Exception ex)
			{
				Logger.Exception(ex);
			}
			finally
			{
			}
			if (m_Count > 0)
			{
				m_Count--;
			}
			if (m_Count == 0)
			{
				break;
			}
		}
		m_LastTime = DateTime.Now - timeSpan;
	}

	public void SetExceptionSpam(int count = 1, int depth = 1, int intervalMs = 0)
	{
		m_Count = count;
		m_Depth = depth;
		m_Interval = TimeSpan.FromMilliseconds(intervalMs);
		m_Type = SpamType.Exceptions;
		m_LastTime = DateTime.Now;
	}

	public void StartSpam(SpamType type, int intervalMs, int depth)
	{
		m_Count = -1;
		m_Depth = depth;
		m_Interval = TimeSpan.FromMilliseconds(intervalMs);
		m_Type = type;
		m_LastTime = DateTime.Now;
	}

	public void StopSpam()
	{
		m_Count = 0;
	}

	private void DebugSpam(SpamType type, int depth)
	{
		depth = Math.Max(depth, 1);
		using SpamLine spamLine = SpamLine.GetFromPool();
		spamLine.Call(type, depth, TotalData + Random.Next(10));
	}

	public static void SpamInternal(SpamType type, int data)
	{
		TotalData = Math.Abs(data / 2) % 97;
		switch (type)
		{
		case SpamType.Warnings:
			Logger.Warning($"debug lifecycle spam, data {data}");
			break;
		case SpamType.Errors:
			Logger.Error($"debug lifecycle spam, data {data}");
			break;
		default:
			throw new Exception($"debug lifecycle spam, data {data}");
		}
	}
}
