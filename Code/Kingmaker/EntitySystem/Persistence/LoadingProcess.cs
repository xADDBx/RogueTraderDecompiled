using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

public class LoadingProcess : MonoBehaviour
{
	private class QueuedProcess
	{
		public IEnumerator Process;

		public Action Callback;

		public LoadingProcessTag Tag;
	}

	private class HideTimeout
	{
		private const float MinTime = 0.15f;

		private const int MinFrames = 4;

		private float m_Time = 100f;

		private int m_Frames = 100;

		public bool IsActive()
		{
			if (m_Frames > 4)
			{
				return m_Time <= 0.15f;
			}
			return true;
		}

		public void Start()
		{
			if (IsActive())
			{
				PFLog.System.Log("Restart hide timer");
			}
			else
			{
				PFLog.System.Log("Start hide timer");
			}
			m_Time = 0f;
			m_Frames = 0;
		}

		public void Cancel()
		{
			if (IsActive())
			{
				PFLog.System.Log("Stop hide timer");
				m_Time = 100f;
				m_Frames = 100;
			}
		}

		public void Tick()
		{
			m_Time += Time.unscaledDeltaTime;
			m_Frames++;
		}
	}

	private static LoadingProcess s_Instance;

	private Stopwatch m_OneTaskStopwatch;

	private Stopwatch m_AllTasksStopwatch;

	[NotNull]
	private HideTimeout m_HideTimeout = new HideTimeout();

	[CanBeNull]
	private ILoadingScreen m_LoadingScreen;

	private IEnumerator m_CurrentLoadingProcess;

	private Action m_OnLoadCallback;

	private Queue<QueuedProcess> m_Queue = new Queue<QueuedProcess>();

	private CountingGuard m_ManualLoadingScreen = new CountingGuard();

	private const float LockedNotificationPeriod = 1f;

	private WarningNotificationType m_LockedNotification;

	private float m_LockedNotificationTimeout;

	private bool m_PostfaceStarted;

	public static LoadingProcess Instance
	{
		get
		{
			if (!s_Instance)
			{
				if (!Application.isPlaying)
				{
					throw new InvalidOperationException("Cannot use LoadingProcess when the game is not playing");
				}
				GameObject obj = new GameObject("LoadingProcess");
				s_Instance = obj.AddComponent<LoadingProcess>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return s_Instance;
		}
	}

	public LoadingProcessTag CurrentProcessTag { get; private set; }

	public bool IsLoadingInProcess
	{
		get
		{
			if (m_CurrentLoadingProcess == null)
			{
				return m_Queue.Count > 0;
			}
			return true;
		}
	}

	public bool IsLoadingScreenActive
	{
		get
		{
			if (m_LoadingScreen != null)
			{
				return m_LoadingScreen.GetLoadingScreenState() != LoadingScreenState.Hidden;
			}
			return false;
		}
	}

	public bool IsLoadingScreenShown
	{
		get
		{
			if (m_LoadingScreen != null)
			{
				return m_LoadingScreen.GetLoadingScreenState() == LoadingScreenState.Shown;
			}
			return false;
		}
	}

	public bool IsManualLoadingScreenActive => m_ManualLoadingScreen;

	public bool IsFadeActive
	{
		get
		{
			if (m_LoadingScreen != null)
			{
				return m_LoadingScreen.Equals(FadeCanvas.Instance);
			}
			return false;
		}
	}

	public CountableFlag IsAwaitingUserInput { get; } = new CountableFlag();


	protected LoadingProcess()
	{
	}

	private void Update()
	{
		if (Runner.IsActive)
		{
			return;
		}
		try
		{
			Tick();
		}
		catch (Exception ex)
		{
			Runner.ReportException(ex);
		}
	}

	public void Tick()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (m_HideTimeout.IsActive())
		{
			m_HideTimeout.Tick();
			if (!m_HideTimeout.IsActive() && m_LoadingScreen != null)
			{
				PFLog.System.Log("Hide loading screen: {0}", m_LoadingScreen);
				m_LoadingScreen.HideLoadingScreen();
			}
			return;
		}
		if (m_LoadingScreen != null)
		{
			switch (m_LoadingScreen.GetLoadingScreenState())
			{
			case LoadingScreenState.Hidden:
				PFLog.System.Log("Hiding loading screen completed: {0}", m_LoadingScreen);
				m_LoadingScreen = null;
				EventBus.RaiseEvent(delegate(ICloseLoadingScreenHandler h)
				{
					h.HandleCloseLoadingScreen();
				});
				break;
			case LoadingScreenState.ShowAnimation:
			case LoadingScreenState.HideAnimation:
				return;
			}
		}
		UpdateLockedNotification();
		try
		{
			TickLoading();
			if (m_LoadingScreen != null && m_CurrentLoadingProcess == null && !m_ManualLoadingScreen && !m_HideTimeout.IsActive())
			{
				m_HideTimeout.Start();
			}
		}
		catch (Exception innerException)
		{
			m_CurrentLoadingProcess = null;
			m_Queue.Clear();
			throw new LoadGameException("", innerException);
		}
	}

	public void TickLoading()
	{
		if (m_CurrentLoadingProcess == null || m_CurrentLoadingProcess.MoveNext())
		{
			return;
		}
		string text = m_CurrentLoadingProcess.GetType().Name;
		m_OneTaskStopwatch.Stop();
		PFLog.System.Log("Loading process finished '{0}' duration: {1}s)", text, m_OneTaskStopwatch.Elapsed.TotalSeconds);
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		LoadingProcessTag currentProcessTag = CurrentProcessTag;
		m_CurrentLoadingProcess = null;
		CurrentProcessTag = LoadingProcessTag.None;
		m_OnLoadCallback?.Invoke();
		stopwatch.Stop();
		PFLog.System.Log("Callback execution duration: {0}s", stopwatch.Elapsed.TotalSeconds);
		if (m_CurrentLoadingProcess != null)
		{
			return;
		}
		if (m_Queue.Count == 0)
		{
			if (!m_PostfaceStarted)
			{
				EnqueueLoadingProcess(Game.AwaitingCommands(), null, currentProcessTag);
			}
			m_PostfaceStarted = !m_PostfaceStarted;
		}
		if (m_Queue.Count > 0)
		{
			QueuedProcess queuedProcess = m_Queue.Dequeue();
			PFLog.System.Log("Dequeue loading process: {0}", queuedProcess.Process.GetType().Name);
			StartLoadingProcessInternal(queuedProcess.Process, queuedProcess.Callback, queuedProcess.Tag);
		}
		else
		{
			m_AllTasksStopwatch.Stop();
			PFLog.System.Log("Loading process is ended in {0}s", m_AllTasksStopwatch.Elapsed.TotalSeconds);
			m_AllTasksStopwatch = null;
		}
	}

	public void StopAll()
	{
		m_CurrentLoadingProcess = null;
		m_OnLoadCallback = null;
		m_Queue.Clear();
		m_ManualLoadingScreen.Reset();
		IsAwaitingUserInput.ReleaseAll();
	}

	public void StartLoadingProcess(IEnumerator process, Action callback = null, LoadingProcessTag processTag = LoadingProcessTag.None)
	{
		if (m_CurrentLoadingProcess != null || m_Queue.Count > 0)
		{
			EnqueueLoadingProcess(process, callback, processTag);
		}
		else
		{
			StartLoadingProcessInternal(process, callback, processTag);
		}
	}

	public void StartLoadingProcess(Action callback, LoadingProcessTag processTag = LoadingProcessTag.None)
	{
		if (m_CurrentLoadingProcess != null || m_Queue.Count > 0)
		{
			IEnumerator<object> enumerator = Enumerable.Empty<object>().GetEnumerator();
			EnqueueLoadingProcess(enumerator, callback, processTag);
		}
		else
		{
			callback?.Invoke();
		}
	}

	private void EnqueueLoadingProcess(IEnumerator process, Action callback = null, LoadingProcessTag processTag = LoadingProcessTag.None)
	{
		m_Queue.Enqueue(new QueuedProcess
		{
			Process = process,
			Callback = callback,
			Tag = processTag
		});
		PFLog.System.Log("Enqueue loading process: {0}s", process.GetType().Name);
	}

	public void ResetLoadingScreen()
	{
		StartLoadingScreen(SelectLoadingScreen(CurrentProcessTag));
	}

	private void StartLoadingProcessInternal(IEnumerator process, Action callback = null, LoadingProcessTag processTag = LoadingProcessTag.None)
	{
		StartLoadingScreen(SelectLoadingScreen(processTag));
		m_OnLoadCallback = callback;
		m_CurrentLoadingProcess = process;
		CurrentProcessTag = processTag;
		SoundState.Instance.ResetState(SoundStateType.LoadingScreen);
		m_OneTaskStopwatch = new Stopwatch();
		m_OneTaskStopwatch.Start();
		PFLog.System.Log("Start loading process: {0}", process.GetType().Name);
		if (m_AllTasksStopwatch == null)
		{
			m_AllTasksStopwatch = new Stopwatch();
			m_AllTasksStopwatch.Start();
		}
	}

	public void ShowManualLoadingScreen(ILoadingScreen loadingScreen)
	{
		PFLog.System.Log("Show manual loading screen: {0}", loadingScreen);
		StartLoadingScreen(loadingScreen);
		++m_ManualLoadingScreen;
	}

	public void HideManualLoadingScreen()
	{
		PFLog.System.Log("Hide manual loading screen");
		--m_ManualLoadingScreen;
	}

	public void ResetManualLoadingScreen()
	{
		PFLog.System.Log("Reset manual loading screen");
		m_ManualLoadingScreen.Reset();
	}

	private void StartLoadingScreen(ILoadingScreen loadingScreen)
	{
		m_HideTimeout.Cancel();
		if (loadingScreen == null)
		{
			return;
		}
		if (m_LoadingScreen != null && m_LoadingScreen.Equals(loadingScreen))
		{
			LoadingScreenState loadingScreenState = m_LoadingScreen.GetLoadingScreenState();
			if (loadingScreenState != 0 && loadingScreenState != LoadingScreenState.HideAnimation)
			{
				return;
			}
			m_LoadingScreen = null;
		}
		if (m_LoadingScreen == null || m_LoadingScreen.Equals(EmptyLoadingScreen.Instance))
		{
			m_LoadingScreen = loadingScreen;
		}
		else if (IsFadeActive && loadingScreen.Equals(Game.Instance.RootUiContext.LoadingScreenRootVM))
		{
			m_LoadingScreen.HideLoadingScreen();
			m_LoadingScreen = loadingScreen;
		}
		LoadingScreenState loadingScreenState2 = m_LoadingScreen.GetLoadingScreenState();
		if (loadingScreenState2 == LoadingScreenState.Hidden || loadingScreenState2 == LoadingScreenState.HideAnimation)
		{
			PFLog.System.Log("Show loading screen: {0}", loadingScreen);
			m_LoadingScreen.ShowLoadingScreen();
			EventBus.RaiseEvent(delegate(IOpenLoadingScreenHandler h)
			{
				h.HandleOpenLoadingScreen();
			});
			SoundState.Instance.OnLoadingScreenShown();
		}
	}

	[CanBeNull]
	private static ILoadingScreen SelectLoadingScreen(LoadingProcessTag tag)
	{
		return tag switch
		{
			LoadingProcessTag.None => Game.Instance.RootUiContext.LoadingScreenRootVM, 
			LoadingProcessTag.Save => EmptyLoadingScreen.Instance, 
			LoadingProcessTag.ReloadMechanics => FadeCanvas.Instance, 
			LoadingProcessTag.ReloadLight => FadeCanvas.Instance, 
			LoadingProcessTag.TeleportParty => FadeCanvas.Instance, 
			LoadingProcessTag.ShowCityBuilder => Game.Instance.RootUiContext.LoadingScreenRootVM, 
			LoadingProcessTag.ExceptionReporter => EmptyLoadingScreen.Instance, 
			LoadingProcessTag.ResetUI => FadeCanvas.Instance, 
			_ => throw new ArgumentOutOfRangeException("tag", tag, "Unknown tag"), 
		};
	}

	public void SetLockedNotification(WarningNotificationType type)
	{
		m_LockedNotification = type;
		m_LockedNotificationTimeout = 0f;
	}

	public void ClearLockedNotification()
	{
		SetLockedNotification(WarningNotificationType.None);
	}

	public bool CanReceiveInput()
	{
		if (IsLoadingInProcess || IsLoadingScreenActive || IsManualLoadingScreenActive)
		{
			return IsAwaitingUserInput;
		}
		return true;
	}

	private void UpdateLockedNotification()
	{
		if (m_LockedNotification == WarningNotificationType.None)
		{
			return;
		}
		m_LockedNotificationTimeout -= Time.unscaledDeltaTime;
		if (m_LockedNotificationTimeout <= 0f)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(m_LockedNotification, addToLog: false);
			});
			m_LockedNotificationTimeout = 1f;
		}
	}

	public IEnumerator FindProcess(Func<IEnumerator, bool> predicate)
	{
		if (m_CurrentLoadingProcess != null && predicate(m_CurrentLoadingProcess))
		{
			return m_CurrentLoadingProcess;
		}
		foreach (QueuedProcess item in m_Queue)
		{
			if (predicate(item.Process))
			{
				return item.Process;
			}
		}
		return null;
	}

	public void CheatRestManualLoadingScreen()
	{
		m_ManualLoadingScreen.Reset();
	}
}
