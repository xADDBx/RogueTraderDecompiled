using System;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;

public class MainMenuSelector : IDelayedSelector
{
	private IDisposable m_DelayedApplySelection;

	public bool IsRunning => m_DelayedApplySelection != null;

	public void InvokeNextFrame(Action action)
	{
		m_DelayedApplySelection = DelayedInvoker.InvokeInFrames(action, 1);
	}

	public void Stop()
	{
		m_DelayedApplySelection?.Dispose();
	}

	public void Clear()
	{
		Stop();
		m_DelayedApplySelection = null;
	}
}
