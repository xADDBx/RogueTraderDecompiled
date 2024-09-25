using System;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class ProfitFactorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IProfitFactorHandler, ISubscriber, IVendorDealPriceChangeHandler
{
	public readonly ReactiveProperty<float> InitialValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> TotalValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> LockedValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> DiffValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> RestoreSpeed = new ReactiveProperty<float>(0f);

	public readonly ReactiveCollection<ProfitFactorModifierVM> Modifiers = new ReactiveCollection<ProfitFactorModifierVM>();

	public ProfitFactorVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		ProfitFactor profitFactor = Game.Instance.Player.ProfitFactor;
		foreach (ProfitFactorModifier item in profitFactor.ModifiersTotal)
		{
			Modifiers.Add(new ProfitFactorModifierVM(item));
		}
		UpdateValues(profitFactor);
	}

	protected override void DisposeImplementation()
	{
		Modifiers.ForEach(delegate(ProfitFactorModifierVM modVm)
		{
			modVm.Dispose();
		});
		Modifiers.Clear();
	}

	private void UpdateValues(ProfitFactor profitFactor)
	{
		InitialValue.Value = profitFactor.InitialValue;
		TotalValue.Value = profitFactor.Total;
		CurrentValue.Value = profitFactor.Total;
		LockedValue.Value = 0f;
	}

	public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
	{
		Modifiers.Add(new ProfitFactorModifierVM(modifier));
		UpdateValues(Game.Instance.Player.ProfitFactor);
	}

	public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
	{
		UpdateValues(Game.Instance.Player.ProfitFactor);
		ProfitFactorModifierVM profitFactorModifierVM = Modifiers.FirstOrDefault((ProfitFactorModifierVM item) => item.Modifier == modifier);
		if (profitFactorModifierVM != null)
		{
			profitFactorModifierVM.Dispose();
			Modifiers.Remove(profitFactorModifierVM);
		}
	}

	public void HandleDealPriceChanged(float dealPrice)
	{
		DiffValue.Value = 0f - dealPrice;
	}
}
