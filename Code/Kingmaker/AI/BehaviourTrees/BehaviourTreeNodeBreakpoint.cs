using System;
using System.Diagnostics;
using UnityEngine;

namespace Kingmaker.AI.BehaviourTrees;

public class BehaviourTreeNodeBreakpoint
{
	private BehaviourTreeBreakpointType _lastActivatedBreakpoint;

	private int _lastActivatedFrame;

	private int _deactivatedFrame;

	private static BehaviourTreeNodeBreakpoint CurrentActiveBreakpoint;

	public BehaviourTreeBreakpointType NodeBreakpoint { get; private set; }

	public bool IsActive { get; private set; }

	public event Action OnBreakpointActiveChange;

	public void ToggleBreakpoint(BehaviourTreeBreakpointType breakpoint)
	{
		NodeBreakpoint ^= breakpoint;
		if (IsActive)
		{
			ForceResetActiveness();
		}
	}

	public bool TryResetActiveness()
	{
		if (!IsActive)
		{
			return true;
		}
		if (_lastActivatedFrame == Time.frameCount)
		{
			return false;
		}
		_deactivatedFrame = Time.frameCount;
		ForceResetActiveness();
		return true;
	}

	public void ForceResetActiveness()
	{
		IsActive = false;
		this.OnBreakpointActiveChange?.Invoke();
		if (CurrentActiveBreakpoint == this)
		{
			CurrentActiveBreakpoint = null;
		}
	}

	public bool TryDebugBreak(BehaviourTreeBreakpointType stage)
	{
		if (CanDebugBreak(stage))
		{
			IsActive = true;
			this.OnBreakpointActiveChange?.Invoke();
			_lastActivatedBreakpoint = stage;
			_lastActivatedFrame = Time.frameCount;
			CurrentActiveBreakpoint = this;
			UnityEngine.Debug.Break();
			Debugger.Break();
			return true;
		}
		return false;
	}

	public bool CanDebugBreak(BehaviourTreeBreakpointType stage)
	{
		if (NodeBreakpoint.HasFlagNonAlloc(stage) && CurrentActiveBreakpoint == null)
		{
			if (_deactivatedFrame == Time.frameCount)
			{
				return _lastActivatedBreakpoint != stage;
			}
			return true;
		}
		return false;
	}

	public static bool IsActiveDebugWillRemainsThisTick()
	{
		BehaviourTreeNodeBreakpoint currentActiveBreakpoint = CurrentActiveBreakpoint;
		if (currentActiveBreakpoint == null || !currentActiveBreakpoint.IsActive)
		{
			return false;
		}
		return CurrentActiveBreakpoint._lastActivatedFrame == Time.frameCount;
	}
}
