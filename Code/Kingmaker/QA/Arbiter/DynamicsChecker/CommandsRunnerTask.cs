using System;
using System.Collections;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.QA.Clockwork;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.DynamicsChecker;

public class CommandsRunnerTask : ArbiterTask
{
	private ClockworkCommandList m_CommandList;

	private GeneralProbeData m_ProbeData;

	private ClockworkCommand m_CurrentCommand;

	private ClockworkRunnerTask m_CurrentTask;

	private float m_Delay;

	private bool m_Finished;

	public CommandsRunnerTask(ArbiterTask parent, ClockworkCommandList commandList, GeneralProbeData probeData)
		: base(parent)
	{
		m_CommandList = commandList;
		m_ProbeData = probeData;
		m_CommandList.Initialize();
	}

	protected override IEnumerator Routine()
	{
		m_Finished = false;
		ArbiterClientMeasurements.StartProfilerRecorders();
		ArbiterClientMeasurements.StartEveryFrameMeasurements();
		EnableImmortality();
		while (!m_Finished)
		{
			ArbiterClientMeasurements.TickEveryFrameMeasurements();
			HandleGameModeChange();
			HandleCameraPosition();
			if (IsWaitingForCutscene())
			{
				yield return null;
				continue;
			}
			if (IsWaitingForDelay())
			{
				yield return null;
				continue;
			}
			SelectNewTaskIfNeeded();
			TickCurrentTask();
			yield return null;
		}
		ArbiterClientMeasurements.StopEveryFrameMeasurements();
		ArbiterClientMeasurements.StopProfilerRecorders();
		DynamicProbeData dynamicProbeData = new DynamicProbeData();
		dynamicProbeData.AddCustomMeasurements(ArbiterClientMeasurements.GetEveryFrameMeasurements());
		m_ProbeData.ProbeDataList.Add(dynamicProbeData);
	}

	private void EnableImmortality()
	{
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			if (!selectedUnit.Buffs.Contains(Game.Instance.BlueprintRoot.Cheats.Iddqd))
			{
				GameHelper.ApplyBuff(selectedUnit, Game.Instance.BlueprintRoot.Cheats.Iddqd);
			}
		}
	}

	private void HandleGameModeChange()
	{
		if (Game.Instance.CurrentMode == GameModeType.Pause)
		{
			PFLog.Clockwork.Log("Unpausing game");
			Game.Instance.IsPaused = false;
		}
		if (Game.Instance.CurrentMode == GameModeType.Default && GameHelper.GetPlayerCharacter().IsInCombat && !(m_CurrentTask is TaskAutoCombat))
		{
			m_CurrentTask = m_CurrentCommand?.GetTask(null);
			m_CurrentTask?.CreateTicker();
			InsertTask(new TaskAutoCombat(null));
		}
	}

	private void HandleCameraPosition()
	{
		bool flag = Game.Instance.CameraController?.Follower.HasTarget ?? false;
		if (Game.Instance.CurrentMode == GameModeType.Default && !CutsceneLock.Active && !flag)
		{
			if (GameHelper.GetPlayerCharacter().IsInCombat)
			{
				Game.Instance.CameraController.Follower.Follow(Game.Instance.TurnController.CurrentUnit);
			}
			else
			{
				Game.Instance.CameraController.Follower.Follow(Game.Instance.Player.MainCharacterEntity);
			}
		}
	}

	private bool IsWaitingForCutscene()
	{
		return Game.Instance.CurrentMode == GameModeType.Cutscene;
	}

	private bool IsWaitingForDelay()
	{
		if (m_Delay > 0f)
		{
			m_Delay -= Time.unscaledDeltaTime;
			m_CurrentTask?.UpdateTimer();
			return true;
		}
		return false;
	}

	private void SelectNewTaskIfNeeded()
	{
		if (m_CurrentTask != null)
		{
			return;
		}
		if (LoadingProcess.Instance.IsLoadingInProcess)
		{
			m_CurrentTask = new TaskDelayedCall(null, null, "wait: Loading in progress", 1f);
		}
		else
		{
			m_CurrentCommand = m_CommandList.GetNextCommand();
			if (m_CurrentCommand != null)
			{
				m_CurrentTask = m_CurrentCommand.GetTask(null);
			}
			else
			{
				m_Finished = true;
			}
		}
		PFLog.Clockwork.Log($"New task: {m_CurrentTask}");
		m_CurrentTask?.CreateTicker();
	}

	private void TickCurrentTask()
	{
		if (m_CurrentTask == null)
		{
			return;
		}
		m_CurrentTask.UpdateTimer();
		if (m_CurrentTask.IsTimeout)
		{
			throw new Exception("Task timeout");
		}
		if (m_CurrentTask.Ticker == null)
		{
			m_CurrentTask.CreateTicker();
		}
		ClockworkRunnerTask currentTask = m_CurrentTask;
		if (m_CurrentTask.Ticker.MoveNext())
		{
			currentTask = (m_CurrentTask.Ticker.Current as ClockworkRunnerTask) ?? currentTask;
		}
		else
		{
			m_CurrentTask.Complete();
			currentTask = m_CurrentTask.Parent;
		}
		if (currentTask != m_CurrentTask)
		{
			m_CurrentTask.Cleanup();
			if (currentTask != null && currentTask.Ticker == null)
			{
				currentTask.Parent = m_CurrentTask;
				currentTask.CreateTicker();
			}
			m_CurrentTask = currentTask;
		}
		else
		{
			m_Delay = (m_CurrentTask.Ticker.Current as float?).GetValueOrDefault();
		}
	}

	private void InsertTask(ClockworkRunnerTask task)
	{
		task.Parent = m_CurrentTask;
		m_CurrentTask?.Cleanup();
		task.CreateTicker();
		m_CurrentTask = task;
	}
}
