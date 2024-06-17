using System.Collections.Generic;
using Kingmaker.TurnBasedMode.Controllers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat.Console;

public class ActionViewElement : MonoBehaviour
{
	[SerializeField]
	private CombatAction.ActionState m_CurrentState;

	[SerializeField]
	public List<ActionViewElementStateTransition> m_ViewElements;

	private bool m_Restricted;

	public CombatAction.ActionState State
	{
		get
		{
			return m_CurrentState;
		}
		set
		{
			m_CurrentState = value;
			Applystate(m_CurrentState, instant: false);
		}
	}

	public bool Restricted
	{
		get
		{
			return m_Restricted;
		}
		set
		{
			m_Restricted = value;
		}
	}

	private void Applystate(CombatAction.ActionState currentState, bool instant)
	{
		m_ViewElements.ForEach(delegate(ActionViewElementStateTransition element)
		{
			element.DoStateTransition(m_CurrentState, instant);
		});
	}

	private void OnValidate()
	{
		Applystate(m_CurrentState, instant: false);
	}
}
