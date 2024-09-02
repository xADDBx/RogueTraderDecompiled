using System;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class ColonyResourceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<BlueprintResource> BlueprintResource = new ReactiveProperty<BlueprintResource>();

	public readonly ReactiveProperty<int> Count = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> CountAdditional = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int> ArrowDirection = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> IsNegative = new ReactiveProperty<bool>();

	public ColonyResourceVM(BlueprintResource blueprintResource, int count, int arrowDirection = 0)
	{
		BlueprintResource.Value = blueprintResource;
		Count.Value = count;
		ArrowDirection.Value = arrowDirection;
	}

	protected override void DisposeImplementation()
	{
	}

	public void UpdateCount(int delta)
	{
		Count.Value += delta;
	}

	public void UpdateCountAdditional(int delta)
	{
		CountAdditional.Value += delta;
	}

	public void UpdateArrowDirection(int arrowDirection = 0)
	{
		ArrowDirection.Value = arrowDirection;
	}
}
