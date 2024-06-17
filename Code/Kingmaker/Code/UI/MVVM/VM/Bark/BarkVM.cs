using System;
using Kingmaker.UI.Sound.Base;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Bark;

public class BarkVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBarkHandle
{
	public readonly string Text;

	private float m_RemainingTime;

	private VoiceOverStatus m_VoiceOverStatus;

	private readonly Action m_DisposeAction;

	public BarkVM(string text, Action disposeAction)
	{
		Text = text;
		m_DisposeAction = disposeAction;
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			Tick();
		}));
	}

	protected override void DisposeImplementation()
	{
	}

	private void Tick()
	{
		m_DisposeAction?.Invoke();
	}

	public bool IsPlayingBark()
	{
		return true;
	}

	public void InterruptBark()
	{
	}
}
