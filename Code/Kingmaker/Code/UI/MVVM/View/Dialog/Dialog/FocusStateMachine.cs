using System;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public struct FocusStateMachine
{
	private Action<FocusState, FocusState> m_Action;

	private bool m_HasFocus;

	private bool m_HasOverlay;

	private FocusState m_State;

	public FocusState State => m_State;

	public FocusStateMachine(Action<FocusState, FocusState> action)
	{
		m_Action = action;
		m_HasFocus = false;
		m_HasOverlay = false;
		m_State = GetState(m_HasFocus, m_HasOverlay);
		m_Action(m_State, m_State);
	}

	public void SetHasFocus(bool value)
	{
		FocusState state = m_State;
		m_HasFocus = value;
		m_State = GetState(m_HasFocus, m_HasOverlay);
		if (state != m_State)
		{
			m_Action(state, m_State);
		}
	}

	public void SetHasOverlay(bool value)
	{
		FocusState state = m_State;
		m_HasOverlay = value;
		m_State = GetState(m_HasFocus, m_HasOverlay);
		if (state != m_State)
		{
			m_Action(state, m_State);
		}
	}

	private static FocusState GetState(bool hasFocus, bool hasOverlay)
	{
		if (!hasFocus)
		{
			return FocusState.None;
		}
		if (!hasOverlay)
		{
			return FocusState.Foreground;
		}
		return FocusState.Background;
	}
}
