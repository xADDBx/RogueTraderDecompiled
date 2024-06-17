using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class AnimationSet : ScriptableObject
{
	[Serializable]
	public class AnimationSequenceAction
	{
		[SerializeField]
		public UnitAnimationType UnitAnimationType;

		[SerializeField]
		public UnitAnimationSpecialAttackType UnitAnimationSpecialAttackType;

		[SerializeField]
		public AnimationClipWrapper AnimationClipWrapper;
	}

	[Serializable]
	public class AnimationSequence
	{
		[SerializeField]
		public List<AnimationSequenceAction> Actions;

		public override string ToString()
		{
			if (Actions == null)
			{
				return "[empty]";
			}
			return string.Join(" - ", Actions.Select((AnimationSequenceAction _attack) => (_attack.UnitAnimationType != UnitAnimationType.SpecialAttack) ? _attack.UnitAnimationType.ToString() : _attack.UnitAnimationSpecialAttackType.ToString()));
		}
	}

	[SerializeField]
	[HideInInspector]
	private AnimationActionBase m_StartupAction;

	[SerializeField]
	[HideInInspector]
	private List<AnimationActionBase> m_Actions = new List<AnimationActionBase>();

	[SerializeField]
	[HideInInspector]
	private List<Transition> m_Transitions = new List<Transition>();

	[SerializeField]
	private List<AnimationSequence> m_Sequences = new List<AnimationSequence>();

	[SerializeField]
	[HideInInspector]
	public UnitAnimationSetSpecialAttacks m_SpecialAttacks;

	[CanBeNull]
	private IReadOnlyDictionary<UnitAnimationType, UnitAnimationAction> m_ActionsByType;

	public List<AnimationActionBase> Actions => m_Actions;

	public IEnumerable<Transition> Transitions => m_Transitions;

	public List<AnimationSequence> Sequences => m_Sequences;

	public AnimationActionBase StartupAction
	{
		get
		{
			return m_StartupAction;
		}
		set
		{
			if (value == null)
			{
				m_StartupAction = null;
				return;
			}
			if (m_Actions.Contains(value))
			{
				m_StartupAction = value;
				return;
			}
			UnityEngine.Debug.LogErrorFormat("Action {0} dont included in this AnimationSet.", value);
		}
	}

	public UnitAnimationAction GetAction(UnitAnimationType type)
	{
		if (m_ActionsByType == null)
		{
			m_ActionsByType = (from v in m_Actions.OfType<UnitAnimationAction>()
				group v by v.Type).ToDictionary((IGrouping<UnitAnimationType, UnitAnimationAction> v) => v.Key, (IGrouping<UnitAnimationType, UnitAnimationAction> v) => v.FirstOrDefault());
		}
		if (!m_ActionsByType.TryGetValue(type, out var value))
		{
			return null;
		}
		return value;
	}

	public UnitAnimationActionSpecialAttack GetSpecialAttack(UnitAnimationSpecialAttackType type)
	{
		if (!m_SpecialAttacks)
		{
			return null;
		}
		return m_SpecialAttacks.GetSpecialAttack(type);
	}

	private void OnValidate()
	{
	}

	public void AddAction(AnimationActionBase action)
	{
		if (m_Actions.Contains(action))
		{
			UnityEngine.Debug.LogErrorFormat("Action {0} already in AnimationSet", action);
		}
		else
		{
			m_Actions.Add(action);
		}
	}

	public void RemoveAction(AnimationActionBase action)
	{
		m_Actions.Remove(action);
		for (int i = 0; i < m_Transitions.Count; i++)
		{
			Transition transition = m_Transitions[i];
			if (transition.FromAction == action || transition.ToAction == action)
			{
				RemoveTransition(transition);
				i--;
			}
		}
		if (m_StartupAction == action)
		{
			m_StartupAction = null;
		}
	}

	public bool AddTransition(Transition transition)
	{
		if (m_Transitions.Any((Transition t) => t.FromClip == transition.FromClip && t.ToClip == transition.ToClip && t.FromAction == transition.FromAction && t.ToAction == transition.ToAction))
		{
			return false;
		}
		m_Transitions.Add(transition);
		return true;
	}

	public bool RemoveTransition(Transition transition)
	{
		return m_Transitions.Remove(transition);
	}
}
