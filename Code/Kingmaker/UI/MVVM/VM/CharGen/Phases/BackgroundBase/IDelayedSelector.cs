using System;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;

public interface IDelayedSelector
{
	bool IsRunning { get; }

	void InvokeNextFrame(Action action);

	void Stop();

	void Clear();
}
