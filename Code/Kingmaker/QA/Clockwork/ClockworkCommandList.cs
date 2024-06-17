using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[Serializable]
public class ClockworkCommandList
{
	[SerializeReference]
	public List<ClockworkCommand> Commands = new List<ClockworkCommand>();

	[HideInInspector]
	public int TestCount;

	[HideInInspector]
	public int SuccessCount;

	[HideInInspector]
	public int FailCount;

	[HideInInspector]
	public int SkipCount;

	private int m_CurrentCommandIdx;

	[HideInInspector]
	public bool IsCompleted => m_CurrentCommandIdx >= Commands.Count;

	public void Initialize()
	{
		m_CurrentCommandIdx = 0;
		foreach (ClockworkCommand command in Commands)
		{
			command.Initialize();
			if (command is ClockworkCheck)
			{
				TestCount++;
			}
		}
		SkipCount = TestCount;
	}

	public ClockworkCommand GetNextCommand()
	{
		while (!IsCompleted)
		{
			ClockworkCommand clockworkCommand = Commands[m_CurrentCommandIdx];
			if (!clockworkCommand.IsCompleted)
			{
				return clockworkCommand;
			}
			m_CurrentCommandIdx++;
		}
		return null;
	}

	public void UpdateComplited()
	{
		while (!IsCompleted && Commands[m_CurrentCommandIdx].IsCompleted)
		{
			m_CurrentCommandIdx++;
		}
	}

	public List<ClockworkCheck> GetAllChecks()
	{
		SuccessCount = 0;
		FailCount = 0;
		SkipCount = 0;
		List<ClockworkCheck> list = new List<ClockworkCheck>();
		foreach (ClockworkCommand command in Commands)
		{
			if (command is ClockworkCheck clockworkCheck)
			{
				list.Add(clockworkCheck);
				if (!clockworkCheck.LastResult.HasValue)
				{
					SkipCount++;
				}
				else if (clockworkCheck.LastResult == true)
				{
					SuccessCount++;
				}
				else
				{
					FailCount++;
				}
			}
		}
		return list;
	}

	public string GetCurrentCommandCaption()
	{
		if (!IsCompleted)
		{
			return Commands[m_CurrentCommandIdx].ToString();
		}
		return string.Empty;
	}
}
