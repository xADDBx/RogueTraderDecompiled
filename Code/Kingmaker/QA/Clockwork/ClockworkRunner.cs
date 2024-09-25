using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tutorial;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public class ClockworkRunner : ITeleportHandler, ISubscriber, IAreaHandler, IGameOverHandler, IGameModeHandler, IPartyCombatHandler, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>
{
	private BlueprintClockworkScenario m_TestScenario;

	private ClockworkRunnerTask m_CurrentTask;

	private ClockworkCommand m_LastCommand;

	private float m_Delay;

	private float m_OnAreaEnterDelay;

	private bool m_IsInStepMode;

	private bool m_IsStepModeTaskGiven;

	private float m_CutsceneTimeout;

	private float m_AreaTimeout;

	private readonly AreaTaskSelector m_AreaTaskSelector = new AreaTaskSelector();

	private List<BaseUnitEntity> m_CurrentEnemies = new List<BaseUnitEntity>();

	public IEnumerable<TargetAndPriority> PrioritizedAreaObjects => m_AreaTaskSelector.PrioritizedAreaTargets;

	public ClockworkAIConfig AIConfig { get; }

	public PlayData Data { get; }

	public float CurrentTaskTimeLeft => CurrentTask?.TimeLeft ?? 0f;

	public float TimeScale { get; set; }

	public BaseUnitEntity Player => Game.Instance.Player.MainCharacterEntity;

	public ClockworkRunnerTask CurrentTask => m_CurrentTask;

	public ClockworkCommand LastCommand => m_LastCommand;

	public float AreaEnterDelay => m_OnAreaEnterDelay;

	public bool Paused { get; set; }

	public ClockworkRunner(BlueprintClockworkScenario testScenario = null, PlayData data = null)
	{
		Data = data ?? new PlayData();
		m_TestScenario = testScenario;
		m_AreaTimeout = m_TestScenario.AreaTimeout;
		AIConfig = new ClockworkAIConfig(new SetAICommand());
		AttemptsCounter.Instance.MaxAttempts = m_TestScenario.TaskMaxAttempts;
		TimeScale = 6f;
	}

	public void Tick()
	{
		try
		{
			if (!Application.isPlaying || (Paused && !m_IsInStepMode) || WaitForAreaInit())
			{
				return;
			}
			HandleGameModeChange();
			HandleCameraPosition();
			CheckAreaTimeout();
			CloseUi();
			if (IsWaitingForCutscene() || IsWaitingForDelay())
			{
				return;
			}
			Clockwork.Instance.Reporter.ReportResults();
			SelectNewTaskIfNeeded();
			TickCurrentTask();
		}
		catch (Exception ex)
		{
			Clockwork.Instance.Reporter.HandleError(ex.Message + "\n\tStackTrace: " + ex.StackTrace);
		}
		if (AIConfig.IsScenarioEnded)
		{
			PFLog.Clockwork.Log("Clockwork Scenario End!");
			EndScenario();
		}
	}

	private void CheckAreaTimeout()
	{
		m_AreaTimeout -= Time.unscaledDeltaTime;
		if (m_AreaTimeout < 0f)
		{
			m_AreaTimeout = m_TestScenario.AreaTimeout;
			Clockwork.Instance.Reporter.HandleError("Area timeout");
		}
	}

	private void CloseUi()
	{
		if (!(m_CurrentTask is TaskInteractWithMapObject) && (Game.Instance.RootUiContext.SurfaceVM?.StaticPartVM?.LootContextVM?.IsShown).GetValueOrDefault())
		{
			MaybeCollectLoot();
		}
		TutorialVM tutorialVM = Game.Instance.RootUiContext.CommonVM?.TutorialVM;
		if (tutorialVM?.BigWindowVM.Value != null)
		{
			tutorialVM.BigWindowVM.Value.Hide();
			PFLog.Clockwork.Log("Closed big tutorial window");
		}
		if (tutorialVM?.SmallWindowVM.Value != null)
		{
			tutorialVM.SmallWindowVM.Value.Hide();
			PFLog.Clockwork.Log("Closed small tutorial window");
		}
		MessageBoxVM messageBoxVM = Game.Instance.RootUiContext.CommonVM?.MessageBoxVM.Value;
		if (messageBoxVM != null)
		{
			PFLog.Clockwork.Log("Accept messagebox with text: " + messageBoxVM.MessageText);
			if (!m_TestScenario.AutoUseRest && messageBoxVM.MessageText.EndsWith("begin resting?"))
			{
				messageBoxVM.OnDeclinePressed();
			}
			else
			{
				messageBoxVM.OnAcceptPressed();
			}
		}
		GroupChangerVM groupChangerVM = Game.Instance.RootUiContext.SurfaceVM?.StaticPartVM?.GroupChangerContextVM.GroupChangerVm.Value;
		if (groupChangerVM != null)
		{
			Game.Instance.GameCommandQueue.AcceptChangeGroup(groupChangerVM.PartyCharacterRef.ToList(), groupChangerVM.RemoteCharacterRef.ToList(), groupChangerVM.RequiredCharactersRef.ToList(), groupChangerVM.IsCapital, groupChangerVM is GroupChangerCommonVM);
		}
	}

	private bool IsWaitingForCutscene()
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			m_CutsceneTimeout -= Time.unscaledDeltaTime;
			if (!(m_CutsceneTimeout < 0f))
			{
				return true;
			}
			m_CutsceneTimeout = m_TestScenario.CutsceneTimeout;
			Clockwork.Instance.Reporter.HandleError("Cutscene timeout");
		}
		m_CutsceneTimeout = m_TestScenario.CutsceneTimeout;
		return false;
	}

	private bool IsWaitingForDelay()
	{
		if (m_Delay > 0f)
		{
			m_Delay -= Game.Instance.TimeController.DeltaTime;
			m_CurrentTask?.UpdateTimer();
			return true;
		}
		return false;
	}

	private void HandleCameraPosition()
	{
		if (Game.Instance.CurrentMode == GameModeType.Default)
		{
			CameraRig.Instance.ScrollTo(Player.Position);
		}
	}

	private void EndScenario()
	{
		Paused = true;
		if (!Clockwork.Instance.ShowDebugEndMessage)
		{
			Clockwork.Instance.Stop();
		}
	}

	public void ForceUpdateTask(bool checkNavMesh = true)
	{
		if (m_TestScenario.IsExplorationMode)
		{
			if (UIUtility.IsGlobalMap())
			{
				m_CurrentTask = null;
			}
			else
			{
				m_CurrentTask = m_AreaTaskSelector.SelectNewTask(this, checkNavMesh);
			}
		}
	}

	private void SelectNewTaskIfNeeded()
	{
		if (m_CurrentTask != null)
		{
			return;
		}
		if (m_IsInStepMode && m_IsStepModeTaskGiven)
		{
			m_IsInStepMode = false;
			m_IsStepModeTaskGiven = false;
			Paused = true;
			return;
		}
		if (LoadingProcess.Instance.IsLoadingInProcess)
		{
			m_CurrentTask = new TaskDelayedCall(this, null, "wait: Loading in progress", 1f);
		}
		else
		{
			ClockworkCommand clockworkCommand = m_TestScenario.GetCommandList()?.GetNextCommand();
			if (clockworkCommand != null)
			{
				m_LastCommand = clockworkCommand;
				m_CurrentTask = clockworkCommand.GetTask(this);
			}
			else if (m_TestScenario.IsReplayMode)
			{
				m_LastCommand = new EndScenarioCommand();
				m_CurrentTask = m_LastCommand.GetTask(this);
			}
			else if (m_TestScenario.IsExplorationMode)
			{
				m_LastCommand = null;
				if (UIUtility.IsGlobalMap())
				{
					m_CurrentTask = null;
				}
				else
				{
					m_CurrentTask = m_AreaTaskSelector.SelectNewTask(this);
				}
			}
		}
		if (!m_IsStepModeTaskGiven)
		{
			m_IsStepModeTaskGiven = true;
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
		if (CurrentTaskTimeLeft < 0f)
		{
			Clockwork.Instance.Reporter.HandleWarning("Task timeout.");
			m_CurrentTask = null;
			return;
		}
		ClockworkRunnerTask currentTask = m_CurrentTask;
		if (currentTask != null && currentTask.TooManyAttempts())
		{
			Clockwork.Instance.Reporter.HandleError("Too many attempts for task.");
			m_CurrentTask = null;
			return;
		}
		if (m_CurrentTask.Ticker == null)
		{
			m_CurrentTask.CreateTicker();
		}
		ClockworkRunnerTask currentTask2 = m_CurrentTask;
		if (m_CurrentTask.Ticker.MoveNext())
		{
			currentTask2 = (m_CurrentTask.Ticker.Current as ClockworkRunnerTask) ?? currentTask2;
		}
		else
		{
			m_CurrentTask.Complete();
			currentTask2 = m_CurrentTask.Parent;
		}
		if (currentTask2 != m_CurrentTask)
		{
			m_CurrentTask.Cleanup();
			if (currentTask2 != null)
			{
				if (currentTask2.Ticker == null)
				{
					currentTask2.Parent = m_CurrentTask;
					currentTask2.CreateTicker();
				}
				PFLog.Clockwork.Log($"Task {m_CurrentTask} switched to {currentTask2}");
			}
			else
			{
				PFLog.Clockwork.Log($"Task {m_CurrentTask} ended");
			}
			m_CurrentTask = currentTask2;
		}
		else
		{
			m_Delay = (m_CurrentTask.Ticker.Current as float?).GetValueOrDefault();
		}
	}

	private void HandleGameModeChange()
	{
		if (Game.Instance.CurrentMode == GameModeType.Pause)
		{
			PFLog.Clockwork.Log("Unpausing game");
			Game.Instance.IsPaused = false;
		}
		if (Game.Instance.CurrentMode == GameModeType.Default && GameHelper.GetPlayerCharacter().IsInCombat && !(m_CurrentTask is TaskWinCombat) && !(m_CurrentTask is TaskHeal))
		{
			PFLog.Clockwork.Log("Interrupt task to enter combat");
			InsertTaskBeforeCurrent(new TaskWinCombat(this));
			m_CurrentTask.CreateTicker();
		}
		if (Game.Instance.CurrentMode == GameModeType.Dialog && !(LastCommand is AnswerCommand))
		{
			DialogController dialogController = Game.Instance.DialogController;
			if (dialogController.Answers.Count() == 1 && dialogController.Answers.First().IsSystem() && m_TestScenario.IsReplayMode)
			{
				if (!(m_CurrentTask is TaskAutoClickSystemDialog))
				{
					InsertTaskBeforeCurrent(new TaskAutoClickSystemDialog(this));
					m_CurrentTask.CreateTicker();
				}
			}
			else if (dialogController.Answers.Any() && m_TestScenario.IsExplorationMode && !(m_CurrentTask is TaskSolveDialog) && !(m_CurrentTask is TaskAutoClickSystemDialog) && !(m_LastCommand is InteractCommand { IsEntityUnit: not false }))
			{
				PFLog.Clockwork.Log("Interrupt task to solve dialog");
				InsertTaskBeforeCurrent(new TaskSolveDialog(this));
			}
		}
		Game.Instance.TimeController.DebugTimeScale = ((Game.Instance.CurrentMode == GameModeType.Cutscene) ? 3f : TimeScale);
	}

	public void MarkAsInteracted(string uniqueId)
	{
		Data.InteractedObjects.Add(uniqueId);
	}

	public void UnmarkAsInteracted(string uniqueId)
	{
		Data.InteractedObjects.Remove(uniqueId);
	}

	public void HandlePartyTeleport(AreaEnterPoint enterPoint)
	{
		Data.UnreachableObjects.Clear();
	}

	public void MaybeCollectLoot()
	{
		LootContextVM lootContextVM = Game.Instance.RootUiContext.SurfaceVM?.StaticPartVM?.LootContextVM;
		if (lootContextVM == null || !lootContextVM.IsShown)
		{
			return;
		}
		lootContextVM.LootVM.Value.LootCollector.CollectAll();
		foreach (ItemEntity item in Player.Inventory)
		{
			item.OnOpenDescriptionFirstTime();
		}
		foreach (ItemEntity item2 in Player.Inventory.ToList())
		{
			if (!item2.Blueprint.IsNotable && !(item2.Blueprint is BlueprintItemKey) && !(item2.Blueprint is BlueprintItemNote) && m_TestScenario.CanSellItem(item2.Blueprint) && item2.Wielder == null && (item2.Enchantments == null || !item2.Enchantments.Any((ItemEnchantment e) => e.Blueprint.AssetGuid != "6b38844e2bffbac48b63036b66e735be")))
			{
				Game.Instance.Player.CargoState.AddToCargo(new List<ItemEntity> { item2 });
				PFLog.Clockwork.Log($"Moved to cargo: {item2}");
			}
		}
		if (lootContextVM.LootVM?.Value != null && lootContextVM.LootVM.Value.Mode == LootContextVM.LootWindowMode.ZoneExit)
		{
			lootContextVM.LootVM.Value.LeaveZone();
		}
	}

	private void InsertTaskBeforeCurrent(ClockworkRunnerTask task)
	{
		if (!(m_CurrentTask is TaskMovePartyToPoint))
		{
			task.Parent = m_CurrentTask;
		}
		m_CurrentTask?.Cleanup();
		task.CreateTicker();
		m_CurrentTask = task;
	}

	public void OnAreaBeginUnloading()
	{
		Data.LocalTransitionUseCount.Clear();
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		PlayData.AreaEntry areaData = Data.GetAreaData(currentlyLoadedArea);
		areaData.Visited = true;
		List<string> mechanicScenes = (from s in currentlyLoadedArea.GetActiveDynamicScenes()
			select s.SceneName).ToList();
		areaData.MechanicSetsSeen.Add(new PlayData.AreaMechanicSet
		{
			MechanicScenes = mechanicScenes
		});
		areaData.LastVisitTime = Game.Instance.TimeController.GameTime;
	}

	public void OnAreaDidLoad()
	{
		PFLog.Clockwork.Log($"Loaded area {Game.Instance.CurrentlyLoadedArea}, wait for area init");
		m_OnAreaEnterDelay = m_TestScenario.OnAreaEnterDelay;
		m_AreaTimeout = m_TestScenario.AreaTimeout;
		m_AreaTaskSelector.SafePoint = Player.Position;
		CheatsCommon.IgnoreEncumbrance = true;
		SettingsRoot.Game.Tutorial.SetValueAndConfirmForAll(value: false);
		Clockwork.SaveClockworkState(Path.Combine(ApplicationPaths.persistentDataPath, "LastSave.json"));
	}

	private bool WaitForAreaInit()
	{
		if (m_OnAreaEnterDelay > 0f)
		{
			m_OnAreaEnterDelay -= Time.unscaledDeltaTime;
			return true;
		}
		return false;
	}

	public void HandleGameOver(Player.GameOverReasonType reason)
	{
		string text = $"GameOver! Reason: {reason}";
		if (reason == Kingmaker.Player.GameOverReasonType.PartyIsDefeated || reason == Kingmaker.Player.GameOverReasonType.EssentialUnitIsDead)
		{
			text = text + "\nEnemies: \n" + string.Join("\n", from x in m_CurrentEnemies
				orderby x.CR descending
				select x.Blueprint);
		}
		Clockwork.Instance.Reporter.HandleWarning(text);
		PFLog.Clockwork.Log(text);
		if (m_TestScenario.IsExplorationMode)
		{
			Clockwork.Instance.LoadSaveOnGameOver();
		}
	}

	public void StepNextCommand()
	{
		m_IsInStepMode = true;
		m_IsStepModeTaskGiven = false;
		Paused = false;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			PFLog.Clockwork.Log("Wait for cutscene");
			m_CutsceneTimeout = m_TestScenario.CutsceneTimeout;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene && m_Delay <= 0f)
		{
			PFLog.Clockwork.Log("Add delay after cutscene");
			m_Delay = TimeScale;
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			m_CurrentEnemies.Clear();
		}
	}

	public void HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.IsPlayerEnemy)
		{
			m_CurrentEnemies.Add(baseUnitEntity);
		}
	}

	public void HandleUnitLeaveCombat()
	{
	}
}
