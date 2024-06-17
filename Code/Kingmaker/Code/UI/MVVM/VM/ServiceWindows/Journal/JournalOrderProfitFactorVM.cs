using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalOrderProfitFactorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<float> Count = new ReactiveProperty<float>();

	public readonly ReactiveProperty<int> CountAdditional = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ArrowDirection = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> IsNegative = new ReactiveProperty<bool>();

	public readonly ProfitFactorVM ProfitFactorVM;

	public JournalOrderProfitFactorVM(int arrowDirection = 0)
	{
		AddDisposable(ProfitFactorVM = new ProfitFactorVM());
		Icon.Value = BlueprintRoot.Instance.UIConfig.UIIcons.ProfitFactor;
		Count.Value = Game.Instance.Player.ProfitFactor.Total;
		ArrowDirection.Value = arrowDirection;
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
	}

	public void UpdateCount(float count)
	{
		Count.Value = count;
	}

	public void SetCountAdditional(int value)
	{
		CountAdditional.Value = value;
	}

	public void UpdateArrowDirection(int arrowDirection = 0)
	{
		ArrowDirection.Value = arrowDirection;
	}

	private void OnUpdateHandler()
	{
		if (Game.Instance.Player != null)
		{
			Count.Value = Game.Instance.Player.ProfitFactor.Total;
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
