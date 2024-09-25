using System;
using Kingmaker.Controllers;
using Kingmaker.Utility.ManualCoroutines;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;

public class InGameSelector : IDelayedSelector
{
	private CoroutineHandler m_DelayedApplySelection;

	public bool IsRunning => m_DelayedApplySelection.IsRunning;

	public void InvokeNextFrame(Action action)
	{
		m_DelayedApplySelection = Game.Instance.CoroutinesController.InvokeInTicks(action, 1);
	}

	public void Stop()
	{
		m_DelayedApplySelection.Stop();
	}

	public void Clear()
	{
		Stop();
		m_DelayedApplySelection = default(CoroutineHandler);
	}
}
