using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class AbilitySelectorWindowVM : SelectorWindowVM<FeatureSelectorSlotVM>
{
	public readonly Action<CharInfoFeatureVM> OnFeatureFocused;

	public AbilitySelectorWindowVM(Action<CharInfoFeatureVM> onConfirm, Action onDecline, BaseUnitEntity unit, Action<CharInfoFeatureVM> onFeatureFocused)
		: base((Action<FeatureSelectorSlotVM>)onConfirm, onDecline, UIUtilityUnit.CollectAbilitiesVMs(unit).ToList(), (ReactiveProperty<FeatureSelectorSlotVM>)null, (EquipSlotVM)null)
	{
		OnFeatureFocused = onFeatureFocused;
	}
}
