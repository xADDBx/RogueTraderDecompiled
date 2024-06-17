using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class BCResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<float> BCResource = new ReactiveProperty<float>();

	public BCResourceVM()
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	private void OnUpdateHandler()
	{
		BCResource.Value = 0f;
	}

	protected override void DisposeImplementation()
	{
	}
}
