using Cinemachine;
using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units.CameraFollow;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class CameraFollowController : IControllerTick, IController, IControllerStop, IControllerEnable, IControllerDisable, ITurnBasedModeHandler, ISubscriber, IGameModeHandler, IAreaHandler
{
	private SurfaceCombatFollowTasksProvider m_TasksProvider;

	private SpaceCombatFollowTasksProvider m_SpaceTasksProvider;

	private readonly TasksQueue<ICameraFollowTask> m_Tasks = new TasksQueue<ICameraFollowTask>();

	private ICameraFollowTask m_CurrentTask;

	private Coroutine m_ScrollToCoroutine;

	private bool m_IsTurnBased;

	private bool m_IsCutscene;

	private bool m_IsSpaceCombat;

	private bool m_IsActionCameraInAction;

	private Quaternion m_ListenerLocalRotation;

	private CinemachineBrain m_Brain;

	private CinemachineTargetGroup m_TargetGroup;

	private CinemachineVirtualCamera m_VcMain;

	private CinemachineVirtualCamera m_VcAction;

	public void OnEnable()
	{
		m_IsTurnBased = Game.Instance.TurnController.TurnBasedModeActive;
		OnGameModeStart(Game.Instance.CurrentMode);
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat)
		{
			m_IsSpaceCombat = true;
			m_SpaceTasksProvider = new SpaceCombatFollowTasksProvider(TryAddTask);
		}
		else
		{
			m_IsSpaceCombat = false;
			m_TasksProvider = new SurfaceCombatFollowTasksProvider(TryAddTask);
		}
	}

	public void OnDisable()
	{
		SetTimescale(1f, force: true);
		if (m_IsSpaceCombat)
		{
			m_SpaceTasksProvider.Dispose();
			m_SpaceTasksProvider = null;
		}
		else
		{
			m_TasksProvider.Dispose();
			m_TasksProvider = null;
		}
	}

	void IControllerStop.OnStop()
	{
		m_Tasks.Clear();
		m_CurrentTask = null;
		m_ScrollToCoroutine = null;
		m_IsTurnBased = false;
		m_IsCutscene = false;
		m_IsSpaceCombat = false;
		CleanActionCamera();
		m_IsActionCameraInAction = false;
		m_ListenerLocalRotation = default(Quaternion);
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!m_IsTurnBased || m_IsCutscene)
		{
			return;
		}
		ICameraFollowTask currentTask = m_CurrentTask;
		if (currentTask == null || !currentTask.IsActive)
		{
			StopTask(m_CurrentTask);
			StartTask((m_Tasks.Count > 0) ? m_Tasks.Dequeue() : null);
		}
		if (m_CurrentTask == null)
		{
			SetTimescale(1f);
			return;
		}
		CameraFollowTaskParamsEntry taskParams = m_CurrentTask.TaskParams;
		if (taskParams != null && taskParams.TimeScale > 0f)
		{
			SetTimescale(m_CurrentTask.TaskParams.TimeScale);
		}
		if (m_IsActionCameraInAction || m_ScrollToCoroutine != null)
		{
			return;
		}
		ICameraFollowTask currentTask2 = m_CurrentTask;
		if (!(currentTask2 is ActionCameraTask))
		{
			if (!(currentTask2 is CameraScrollToTask))
			{
				if (!(currentTask2 is CameraFollowTask))
				{
					_ = currentTask2 is DoNothingWait;
				}
				else
				{
					Game.Instance.CameraController?.Follower.Follow(m_CurrentTask.Owner?.Entity);
				}
			}
			else
			{
				CameraScrollTo();
			}
		}
		else
		{
			StartActionCamera((ActionCameraTask)m_CurrentTask);
		}
	}

	private void StartTask(ICameraFollowTask task)
	{
		m_CurrentTask = task;
		m_CurrentTask?.Start();
		m_ScrollToCoroutine = null;
	}

	private void StopTask(ICameraFollowTask task)
	{
		if (task != null && task is ActionCameraTask)
		{
			StopActionCamera();
		}
	}

	private void TryAddTask(ICameraFollowTask task)
	{
		if (m_IsTurnBased)
		{
			ICameraFollowTask result;
			if (m_CurrentTask != null && m_CurrentTask.Priority < task.Priority)
			{
				m_Tasks.AddFirst(m_CurrentTask);
				StartTask(task);
			}
			else if (m_CurrentTask != null && CompareTasks(m_CurrentTask, task))
			{
				m_CurrentTask.Reset(task.TaskParams.CameraObserveTime);
			}
			else if (m_Tasks.TryFind((ICameraFollowTask t) => CompareTasks(t, task), out result))
			{
				result.Reset(task.TaskParams.CameraObserveTime);
			}
			else
			{
				m_Tasks.Enqueue(task);
			}
		}
	}

	private static void SetTimescale(float scale, bool force = false)
	{
		if (force || !Mathf.Approximately(scale, Game.Instance.TimeController.CameraFollowTimeScale))
		{
			Game.Instance.GameCommandQueue.CameraFollowTimeScale(scale, force);
		}
	}

	private static bool CompareTasks(ICameraFollowTask task, ICameraFollowTask otherTask)
	{
		if (task != null)
		{
			return task.Owner == otherTask.Owner;
		}
		return false;
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		m_IsTurnBased = isTurnBased;
		if (!isTurnBased)
		{
			UIAccess.SelectionManager.SelectAll();
			SetTimescale(1f, force: true);
		}
	}

	private void CameraScrollTo()
	{
		if (m_CurrentTask.TaskState == CameraFollowTaskState.CameraFly)
		{
			CameraFlyAnimationParams cameraFlyParams = m_CurrentTask.TaskParams.CameraFlyParams;
			float maxSpeed = (cameraFlyParams.AutoSpeed ? float.MaxValue : cameraFlyParams.MaxSpeed);
			float speed = (cameraFlyParams.AutoSpeed ? 0f : cameraFlyParams.Speed);
			ICameraFollowTask currentTask = m_CurrentTask;
			m_ScrollToCoroutine = Game.Instance.CameraController?.Follower.ScrollToTimed(m_CurrentTask.Position, cameraFlyParams.MaxTime, maxSpeed, speed, cameraFlyParams.AnimationCurve, useUnscaledTime: true, delegate
			{
				if (m_CurrentTask == currentTask)
				{
					m_ScrollToCoroutine = null;
					m_CurrentTask.SetTaskState(CameraFollowTaskState.Observe);
				}
			});
		}
		else
		{
			Game.Instance.CameraController?.Follower.ScrollTo(m_CurrentTask.Position);
		}
	}

	private void StartActionCamera(ActionCameraTask task)
	{
		m_TargetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
		m_IsActionCameraInAction = true;
		CameraRig instance = CameraRig.Instance;
		if (task.Caster == task.Target)
		{
			m_TargetGroup.AddMember(task.Caster.View.ViewTransform, 1f, 0f);
			SetVirtualCameraSettingsCaster();
		}
		else
		{
			if (Vector3.Distance(instance.Camera.transform.position, task.Caster.View.ViewTransform.position) < Vector3.Distance(instance.Camera.transform.position, task.Target.View.ViewTransform.position))
			{
				m_TargetGroup.AddMember(task.Caster.View.ViewTransform, 1f, 0f);
				m_TargetGroup.AddMember(task.Target.View.ViewTransform, 1f, 0f);
			}
			else
			{
				m_TargetGroup.AddMember(task.Target.View.ViewTransform, 1f, 0f);
				m_TargetGroup.AddMember(task.Caster.View.ViewTransform, 1f, 0f);
			}
			m_VcAction.LookAt = m_TargetGroup.transform;
			SetVirtualCameraSettingsGroup();
		}
		instance.LockCamera();
		m_ListenerLocalRotation = instance.SetAudioListenerForActionCamera();
		m_VcAction.enabled = true;
	}

	private void StopActionCamera()
	{
		m_IsActionCameraInAction = false;
		CameraRig instance = CameraRig.Instance;
		instance.ResetAudioListenerAfterActionCamera(m_ListenerLocalRotation);
		m_VcAction.enabled = false;
		instance.UnLockCamera();
	}

	private void SetVirtualCameraSettingsCaster()
	{
		Vector3 position = CameraRig.Instance.Camera.transform.position;
		Vector3 position2 = m_TargetGroup.m_Targets[0].target.position;
		Vector3 normalized = (position2 - position).normalized;
		m_VcAction.transform.position = m_TargetGroup.m_Targets[0].target.position;
		m_VcAction.transform.rotation = Quaternion.LookRotation(normalized);
		float num = Vector3.Distance(position, position2) / 3f;
		m_VcAction.transform.localPosition += m_VcAction.transform.forward * (num * -1f);
		BlueprintActionCameraSettings actionCameraSettings = BlueprintRoot.Instance.ActionCameraSettings;
		if (actionCameraSettings != null)
		{
			CinemachineGroupComposer cinemachineComponent = m_VcAction.GetCinemachineComponent<CinemachineGroupComposer>();
			cinemachineComponent.m_GroupFramingSize = actionCameraSettings.GroupFramingSize;
			cinemachineComponent.m_AdjustmentMode = actionCameraSettings.AdjustmentMode;
			cinemachineComponent.m_MinimumFOV = actionCameraSettings.MinimumFOW;
			cinemachineComponent.m_MaximumFOV = actionCameraSettings.MaximumFOV;
			cinemachineComponent.m_MinimumDistance = actionCameraSettings.MinimumDistance;
			cinemachineComponent.m_MaximumDistance = actionCameraSettings.MaximumDistance;
			cinemachineComponent.m_HorizontalDamping = (cinemachineComponent.m_VerticalDamping = 0f);
			cinemachineComponent.m_FrameDamping = actionCameraSettings.FrameDamping;
			if (actionCameraSettings.NoiseSettings != null)
			{
				CinemachineBasicMultiChannelPerlin cinemachineComponent2 = m_VcAction.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
				cinemachineComponent2.m_NoiseProfile = actionCameraSettings.NoiseSettings;
				cinemachineComponent2.m_FrequencyGain = actionCameraSettings.FrequencyGain;
				cinemachineComponent2.m_AmplitudeGain = actionCameraSettings.AmplitudeGain;
			}
		}
	}

	private void SetVirtualCameraSettingsGroup()
	{
		Vector3 normalized = (m_TargetGroup.m_Targets[1].target.position - m_TargetGroup.m_Targets[0].target.position).normalized;
		m_VcAction.transform.position = m_TargetGroup.m_Targets[0].target.position;
		m_VcAction.transform.rotation = Quaternion.LookRotation(normalized);
		float num = Vector3.Distance(m_TargetGroup.m_Targets[0].target.position, m_TargetGroup.m_Targets[1].target.position) / 3f;
		Vector3 position = CameraRig.Instance.Camera.transform.position;
		float num2 = ((Vector3.Distance(position, m_VcAction.transform.right) < Vector3.Distance(position, m_VcAction.transform.right * -1f)) ? 1f : (-1f));
		m_VcAction.transform.localPosition += m_VcAction.transform.right * (num * num2);
		m_VcAction.transform.localPosition += m_VcAction.transform.up * num;
		m_VcAction.transform.localPosition += m_VcAction.transform.forward * num;
		BlueprintActionCameraSettings actionCameraSettings = BlueprintRoot.Instance.ActionCameraSettings;
		if (actionCameraSettings != null)
		{
			CinemachineGroupComposer cinemachineComponent = m_VcAction.GetCinemachineComponent<CinemachineGroupComposer>();
			cinemachineComponent.m_GroupFramingSize = actionCameraSettings.GroupFramingSize;
			cinemachineComponent.m_AdjustmentMode = actionCameraSettings.AdjustmentMode;
			cinemachineComponent.m_MinimumFOV = actionCameraSettings.MinimumFOW;
			cinemachineComponent.m_MaximumFOV = actionCameraSettings.MaximumFOV;
			cinemachineComponent.m_MinimumDistance = actionCameraSettings.MinimumDistance;
			cinemachineComponent.m_MaximumDistance = actionCameraSettings.MaximumDistance;
			cinemachineComponent.m_HorizontalDamping = (cinemachineComponent.m_VerticalDamping = 0f);
			cinemachineComponent.m_FrameDamping = actionCameraSettings.FrameDamping;
			if (actionCameraSettings.NoiseSettings != null)
			{
				CinemachineBasicMultiChannelPerlin cinemachineComponent2 = m_VcAction.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
				cinemachineComponent2.m_NoiseProfile = actionCameraSettings.NoiseSettings;
				cinemachineComponent2.m_FrequencyGain = actionCameraSettings.FrequencyGain;
				cinemachineComponent2.m_AmplitudeGain = actionCameraSettings.AmplitudeGain;
			}
		}
	}

	private void InitializeCameras()
	{
		if (!m_Brain || !m_VcMain || !m_VcAction)
		{
			CameraRig instance = CameraRig.Instance;
			if (m_Brain == null)
			{
				m_Brain = instance.Camera.gameObject.AddComponent<CinemachineBrain>();
			}
			BlueprintActionCameraSettings actionCameraSettings = BlueprintRoot.Instance.ActionCameraSettings;
			if (m_TargetGroup == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = "CinemachineTargetGroup";
				gameObject.transform.parent = instance.ActionCameraRoot.transform;
				m_TargetGroup = gameObject.AddComponent<CinemachineTargetGroup>();
				m_TargetGroup.transform.ResetAll();
			}
			if (m_VcMain == null)
			{
				GameObject gameObject2 = new GameObject();
				gameObject2.name = "VcMain";
				gameObject2.transform.parent = instance.CameraAttachPoint;
				m_VcMain = gameObject2.AddComponent<CinemachineVirtualCamera>();
				m_VcMain.transform.ResetAll();
				m_VcMain.m_Lens.FieldOfView = 30f;
			}
			if (m_VcAction == null)
			{
				GameObject gameObject3 = new GameObject();
				gameObject3.name = "VcAction";
				gameObject3.transform.parent = instance.ActionCameraRoot.transform;
				m_VcAction = gameObject3.AddComponent<CinemachineVirtualCamera>();
			}
			m_Brain.m_CustomBlends = actionCameraSettings.BrainCustomBlends;
			m_VcAction.AddCinemachineComponent<CinemachineGroupComposer>();
			m_VcAction.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			m_VcAction.enabled = false;
		}
	}

	private void CleanActionCamera()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.SpaceCombat)
		{
			if (m_IsActionCameraInAction)
			{
				StopActionCamera();
			}
			if ((bool)m_Brain)
			{
				Object.Destroy(m_Brain);
			}
			if ((bool)m_TargetGroup)
			{
				Object.Destroy(m_TargetGroup.gameObject);
			}
			if ((bool)m_VcMain)
			{
				Object.Destroy(m_VcMain.gameObject);
			}
			if ((bool)m_VcAction)
			{
				Object.Destroy(m_VcAction.gameObject);
			}
			m_Brain = null;
			m_TargetGroup = null;
			m_VcMain = null;
			m_VcAction = null;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsCutscene = gameMode == GameModeType.Cutscene || gameMode == GameModeType.CutsceneGlobalMap;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat)
		{
			InitializeCameras();
		}
	}
}
