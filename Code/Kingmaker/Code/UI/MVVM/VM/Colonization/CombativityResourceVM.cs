using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization;

public class CombativityResourceVM : ColonyResourceVM, ICombativityHandler, ISubscriber
{
	public readonly ReactiveProperty<float> InitialValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> TotalValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> LockedValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> DiffValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveCollection<ProfitFactorModifierVM> Modifiers = new ReactiveCollection<ProfitFactorModifierVM>();

	public CombativityResourceVM(BlueprintResource blueprintResource, int count, int arrowDirection = 0)
		: base(blueprintResource, count, arrowDirection)
	{
		AddDisposable(EventBus.Subscribe(this));
		Combativity combativity = Game.Instance.Player.Combativity;
		foreach (CombativityModifier item in combativity.ModifiersTotal)
		{
			Modifiers.Add(new ProfitFactorModifierVM(item));
		}
		UpdateValues(combativity);
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Modifiers.ForEach(delegate(ProfitFactorModifierVM modVm)
		{
			modVm.Dispose();
		});
		Modifiers.Clear();
	}

	private void UpdateValues(Combativity combativity)
	{
		InitialValue.Value = combativity.InitialValue;
		TotalValue.Value = combativity.Total;
		CurrentValue.Value = combativity.Total;
		LockedValue.Value = 0f;
		SetCount((int)combativity.Total);
	}

	public void HandleCombativityModifierAdded(float max, CombativityModifier modifier)
	{
		Modifiers.Add(new ProfitFactorModifierVM(modifier));
		UpdateValues(Game.Instance.Player.Combativity);
	}

	public void HandleCombativityModifierRemoved(float max, CombativityModifier modifier)
	{
		UpdateValues(Game.Instance.Player.Combativity);
		ProfitFactorModifierVM profitFactorModifierVM = Modifiers.FirstOrDefault((ProfitFactorModifierVM item) => item.Modifier == modifier);
		if (profitFactorModifierVM != null)
		{
			profitFactorModifierVM.Dispose();
			Modifiers.Remove(profitFactorModifierVM);
		}
	}
}
