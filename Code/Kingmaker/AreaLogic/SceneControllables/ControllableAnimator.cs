using System;
using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.Networking;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[RequireComponent(typeof(Animator))]
public class ControllableAnimator : ControllableComponent, IUpdatable, IInterpolatable
{
	[Serializable]
	public class ActionsOnAnimationEvent
	{
		public int EventId;

		[SerializeField]
		public ActionList Actions = new ActionList();
	}

	[Serializable]
	public class ActionHoldersOnAnimationEvent
	{
		public int EventId;

		[SerializeField]
		public List<ActionsReference> ActionHolders = new List<ActionsReference>();
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("ControllableComponent");

	private const int SubTickCount = 16;

	private Animator m_Animator;

	private static readonly int State = Animator.StringToHash("State");

	[ShowIf("m_ObsoleteEventsExist")]
	[Obsolete]
	public List<ActionsOnAnimationEvent> ActionsOnEvent = new List<ActionsOnAnimationEvent>();

	[Obsolete]
	private Dictionary<int, ActionList> m_ActionsMapObsolete = new Dictionary<int, ActionList>();

	public List<ActionHoldersOnAnimationEvent> ActionHoldersOnEvent = new List<ActionHoldersOnAnimationEvent>();

	private Dictionary<int, List<ActionsReference>> m_ActionHolders = new Dictionary<int, List<ActionsReference>>();

	private float m_SubTickDeltaTime;

	private int m_SubTicksRemains;

	private readonly List<int> m_EventsDuringSubTicks = new List<int>();

	private readonly List<ControllableState> m_StatesRequestQueue = new List<ControllableState>();

	private bool m_Enabled;

	private bool m_Initialized;

	private bool m_NeedUpdate;

	[SerializeField]
	[Tooltip("Используется если этот контроллабл двигает геометрию, на которой стоят юниты (лифт, движущаяся платформа). На загрузке сейва navmesh-снэп юнитов будет отложен до тех пор, пока аниматор не доедет до сохранённого состояния(т.е его стейт аниматора совпадает со стейтом на момент сохранения).")]
	private bool m_WaitForStateOnLoad;

	private bool m_HasSettledAfterLoad = true;

	private int m_SettleStartTick;

	private bool m_ObsoleteEventsExist => ActionsOnEvent.Count > 0;

	protected override void Awake()
	{
		base.Awake();
		Setup();
	}

	private void Update()
	{
		if (m_NeedUpdate)
		{
			m_Animator.Update(0f);
			m_NeedUpdate = false;
		}
	}

	public void Setup()
	{
		if (!m_Initialized)
		{
			if (string.IsNullOrEmpty(UniqueId))
			{
				ResetUniqueId();
				PFLog.Default.Log("[ControllableAnimator] Generated new UniqueId: " + UniqueId + " for " + base.gameObject.name);
			}
			m_Initialized = true;
			if (m_Animator == null)
			{
				m_Animator = GetComponent<Animator>();
				m_Animator.enabled = false;
				m_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
				m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			}
			SetupActions();
			ScheduleUpdate();
		}
	}

	public bool TryArmLoadSettleGate()
	{
		if (!m_WaitForStateOnLoad)
		{
			return false;
		}
		m_HasSettledAfterLoad = false;
		m_SettleStartTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		return true;
	}

	protected override void OnDestroy()
	{
		m_Initialized = false;
		if (m_WaitForStateOnLoad && !m_HasSettledAfterLoad)
		{
			m_HasSettledAfterLoad = true;
			Game.Instance?.SceneControllables.NotifySettled(this);
		}
		UnscheduleUpdate();
		base.OnDestroy();
	}

	private void ScheduleUpdate()
	{
		Game.Instance?.CustomUpdateBeforePhysicsController.AddUnique(this);
		Game.Instance?.InterpolationController.AddUnique(this);
	}

	private void UnscheduleUpdate()
	{
		Game.Instance?.CustomUpdateBeforePhysicsController.Remove(this);
		Game.Instance?.InterpolationController.Remove(this);
	}

	private void Clear()
	{
		m_EventsDuringSubTicks.Clear();
		m_StatesRequestQueue.Clear();
		m_SubTickDeltaTime = 0f;
		m_SubTicksRemains = 0;
	}

	public override void SetState(ControllableState state)
	{
		base.SetState(state);
		m_StatesRequestQueue.Add(state);
	}

	void IUpdatable.Tick(float delta)
	{
		if (!m_Initialized)
		{
			return;
		}
		if (!CheckGameObjectAndAnimator())
		{
			OnDestroy();
			return;
		}
		using (ProfileScope.New("ControllableAnimator Sim Tick"))
		{
			if (m_Enabled)
			{
				if (NetworkingManager.IsActive)
				{
					for (int i = 0; i < m_SubTicksRemains; i++)
					{
						m_Animator.Update(m_SubTickDeltaTime);
					}
				}
				else
				{
					m_Animator.Update(m_SubTickDeltaTime * (float)m_SubTicksRemains);
				}
				HandleEvents();
			}
			HandleStateRequests();
			m_Enabled = base.gameObject.activeInHierarchy;
			if (!m_Enabled)
			{
				if (m_WaitForStateOnLoad && !m_HasSettledAfterLoad)
				{
					m_HasSettledAfterLoad = true;
					Game.Instance?.SceneControllables.NotifySettled(this);
				}
				Clear();
				return;
			}
			if (NetworkingManager.IsActive)
			{
				m_Animator.Update(0f);
			}
			if (m_WaitForStateOnLoad)
			{
				HandleSettleAndHashUpdate();
			}
			m_SubTickDeltaTime = delta / 16f;
			m_SubTicksRemains = 16;
		}
	}

	private void HandleSettleAndHashUpdate()
	{
		int fullPathHash = m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		if (!m_HasSettledAfterLoad)
		{
			int? num = GetState()?.SavedAnimatorStateHash;
			bool num2 = !num.HasValue;
			bool flag = !num2 && num.Value == fullPathHash;
			bool flag2 = Game.Instance.RealTimeController.CurrentNetworkTick - m_SettleStartTick >= 40;
			if (num2 || flag || flag2)
			{
				m_HasSettledAfterLoad = true;
				if (flag2 && !flag)
				{
					PFLog.Default.Error("ControllableAnimator '" + base.gameObject.name + "' did not reach saved animator state " + $"within {40} network ticks " + $"(saved={num} current={fullPathHash}). Animator graph may have changed " + "or saved state is unreachable. Releasing settle gate by timeout.");
				}
				Game.Instance?.SceneControllables.NotifySettled(this);
			}
		}
		else
		{
			Game.Instance?.SceneControllables.UpdateSavedHash(UniqueId, fullPathHash);
		}
	}

	private bool CheckGameObjectAndAnimator()
	{
		try
		{
			return base.gameObject != null && m_Animator != null;
		}
		catch (Exception)
		{
			return false;
		}
	}

	void IInterpolatable.Tick(float progress)
	{
		using (ProfileScope.New("ControllableAnimator Int Tick"))
		{
			if (!m_Enabled)
			{
				return;
			}
			int num = 16 - m_SubTicksRemains;
			int value = Mathf.RoundToInt(16f * progress) - num;
			value = Mathf.Clamp(value, 0, m_SubTicksRemains);
			if (value == 0)
			{
				return;
			}
			m_SubTicksRemains -= value;
			if (NetworkingManager.IsActive)
			{
				for (int i = 0; i < value; i++)
				{
					m_Animator.Update(m_SubTickDeltaTime);
				}
			}
			else
			{
				m_Animator.Update(m_SubTickDeltaTime * (float)value);
			}
			m_NeedUpdate = true;
		}
	}

	private void SetupActions()
	{
		foreach (ActionsOnAnimationEvent item in ActionsOnEvent)
		{
			m_ActionsMapObsolete[item.EventId] = item.Actions;
		}
		foreach (ActionHoldersOnAnimationEvent item2 in ActionHoldersOnEvent)
		{
			if (m_ActionHolders.TryGetValue(item2.EventId, out var value))
			{
				value.AddRange(item2.ActionHolders);
			}
			else
			{
				m_ActionHolders[item2.EventId] = item2.ActionHolders;
			}
		}
	}

	private void HandleEvents()
	{
		foreach (int eventsDuringSubTick in m_EventsDuringSubTicks)
		{
			if (m_ActionsMapObsolete.TryGetValue(eventsDuringSubTick, out var value))
			{
				value?.Run();
			}
			if (!m_ActionHolders.TryGetValue(eventsDuringSubTick, out var value2))
			{
				continue;
			}
			foreach (ActionsReference item in value2)
			{
				item?.Get()?.Run();
			}
		}
		m_EventsDuringSubTicks.Clear();
	}

	private void HandleStateRequests()
	{
		foreach (ControllableState item in m_StatesRequestQueue)
		{
			if (item.State.HasValue)
			{
				m_Animator.SetInteger(State, item.State.Value);
			}
			if (item.Active.HasValue)
			{
				base.gameObject.SetActive(item.Active.Value);
			}
		}
		m_StatesRequestQueue.Clear();
	}

	public void RunActions(int eventId)
	{
		if (m_ActionsMapObsolete.ContainsKey(eventId))
		{
			m_EventsDuringSubTicks.Add(eventId);
		}
		if (m_ActionHolders.ContainsKey(eventId))
		{
			m_EventsDuringSubTicks.Add(eventId);
		}
	}
}
