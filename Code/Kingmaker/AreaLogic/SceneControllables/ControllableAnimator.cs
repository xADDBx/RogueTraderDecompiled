using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[RequireComponent(typeof(Animator))]
public class ControllableAnimator : ControllableComponent
{
	[Serializable]
	public class ActionsOnAnimationEvent
	{
		public int EventId;

		[SerializeField]
		public ActionList Actions = new ActionList();
	}

	private Animator m_Animator;

	private static readonly int State = Animator.StringToHash("State");

	public List<ActionsOnAnimationEvent> ActionsOnEvent = new List<ActionsOnAnimationEvent>();

	private Dictionary<int, ActionList> m_ActionsMap = new Dictionary<int, ActionList>();

	protected override void Awake()
	{
		base.Awake();
		m_Animator = GetComponent<Animator>();
		foreach (ActionsOnAnimationEvent item in ActionsOnEvent)
		{
			m_ActionsMap[item.EventId] = item.Actions;
		}
	}

	public override void SetState(ControllableState state)
	{
		base.SetState(state);
		m_Animator.SetInteger(State, state.State);
	}

	public void RunActions(int eventId)
	{
		if (m_ActionsMap.TryGetValue(eventId, out var value))
		{
			value.Run();
		}
	}
}
