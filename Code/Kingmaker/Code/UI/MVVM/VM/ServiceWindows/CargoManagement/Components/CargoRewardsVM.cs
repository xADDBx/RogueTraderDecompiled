using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Cargo;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class CargoRewardsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly AutoDisposingReactiveCollection<CargoRewardSlotVM> CargoRewards = new AutoDisposingReactiveCollection<CargoRewardSlotVM>();

	public readonly ReactiveCommand UpdateCargo = new ReactiveCommand();

	private Action m_CloseCallback;

	public CargoRewardsVM(List<BlueprintCargo> cargoes, Action closeCallback)
	{
		m_CloseCallback = closeCallback;
		SetCargo(cargoes);
	}

	public CargoRewardsVM(IEnumerable<CargoEntity> cargoes, Action closeCallback)
	{
		m_CloseCallback = closeCallback;
		SetCargo(cargoes.ToList());
	}

	protected override void DisposeImplementation()
	{
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
		m_CloseCallback = null;
	}

	public void AddCargo(List<CargoEntity> cargoes)
	{
		AddCargoInternal(cargoes);
	}

	private void SetCargo(List<BlueprintCargo> cargoes)
	{
		CargoRewards.Clear();
		foreach (BlueprintCargo blueprintCargo in cargoes)
		{
			CargoRewardSlotVM cargoRewardSlotVM = CargoRewards.FirstOrDefault((CargoRewardSlotVM c) => c.Origin == blueprintCargo.OriginType);
			if (cargoRewardSlotVM != null)
			{
				cargoRewardSlotVM.IncreaseCount();
				continue;
			}
			CargoRewardSlotVM cargoRewardSlotVM2 = new CargoRewardSlotVM(blueprintCargo);
			AddDisposable(cargoRewardSlotVM2);
			CargoRewards.Add(cargoRewardSlotVM2);
		}
		UpdateCargo.Execute();
	}

	private void SetCargo(List<CargoEntity> cargoes)
	{
		CargoRewards.Clear();
		AddCargoInternal(cargoes);
	}

	private void AddCargoInternal(List<CargoEntity> cargoes)
	{
		foreach (CargoEntity cargo in cargoes)
		{
			CargoRewardSlotVM cargoRewardSlotVM = CargoRewards.FirstOrDefault((CargoRewardSlotVM c) => c.Origin == cargo.Blueprint.OriginType);
			if (cargoRewardSlotVM != null)
			{
				cargoRewardSlotVM.IncreaseCount();
				continue;
			}
			CargoRewardSlotVM cargoRewardSlotVM2 = new CargoRewardSlotVM(cargo);
			AddDisposable(cargoRewardSlotVM2);
			CargoRewards.Add(cargoRewardSlotVM2);
		}
		UpdateCargo.Execute();
	}
}
