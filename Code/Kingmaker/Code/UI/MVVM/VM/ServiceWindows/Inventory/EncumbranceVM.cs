using System;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class EncumbranceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<int> Light = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> Medium = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> Heavy = new ReactiveProperty<int>(1);

	public readonly ReactiveProperty<float> CurrentWeight = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentWeightRatio = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<string> LoadStatus = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> LoadWeight = new ReactiveProperty<string>(string.Empty);

	public EncumbranceVM()
	{
	}

	public EncumbranceVM(EncumbranceHelper.CarryingCapacity capacity)
	{
		SetCapacity(capacity);
	}

	public void SetCapacity(EncumbranceHelper.CarryingCapacity capacity)
	{
		Heavy.Value = capacity.Heavy;
		Light.Value = capacity.Light;
		Medium.Value = capacity.Medium;
		CurrentWeight.Value = capacity.CurrentWeight;
		CurrentWeightRatio.Value = capacity.CurrentWeight / (float)capacity.Heavy;
		LoadStatus.Value = capacity.GetEncumbranceText();
		LoadWeight.Value = $"{capacity.GetCurrentWeightText()}/{Mathf.Max(capacity.Heavy, 0f):0.#}";
	}

	protected override void DisposeImplementation()
	{
	}
}
