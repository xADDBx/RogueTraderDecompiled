using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.GameOver;

public class EndOfGameVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_OnDispose;

	public EndOfGameVM(Action onDispose)
	{
		m_OnDispose = onDispose;
	}

	public void Close()
	{
		m_OnDispose?.Invoke();
	}

	protected override void DisposeImplementation()
	{
	}
}
