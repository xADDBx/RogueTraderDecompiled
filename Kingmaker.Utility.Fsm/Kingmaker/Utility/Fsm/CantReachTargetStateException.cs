using System;

namespace Kingmaker.Utility.Fsm;

public class CantReachTargetStateException : Exception
{
	private readonly Enum m_TargetState;

	private readonly Enum m_ActualState;

	public override string Message => $"{base.Message}{Environment.NewLine}Reach {m_ActualState} instead of {m_TargetState}";

	public CantReachTargetStateException(Enum targetState, Enum actualState)
	{
		m_TargetState = targetState;
		m_ActualState = actualState;
	}
}
