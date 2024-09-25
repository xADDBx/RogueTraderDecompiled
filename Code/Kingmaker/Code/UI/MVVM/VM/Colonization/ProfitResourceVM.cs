using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class ProfitResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<float> ProfitResource = new ReactiveProperty<float>();

	public ProfitResourceVM()
	{
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	private void OnUpdateHandler()
	{
		ProfitResource.Value = Game.Instance.Player.ProfitFactor.Total;
	}

	protected override void DisposeImplementation()
	{
	}
}
