using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class AbilitySelectorWindowVM : SelectorWindowVM<CharInfoFeatureVM>
{
	public AbilitySelectorWindowVM(Action<CharInfoFeatureVM> onConfirm, Action onDecline, BaseUnitEntity unit)
		: base(onConfirm, onDecline, UIUtilityUnit.CollectAbilitiesVMs(unit).ToList(), (ReactiveProperty<CharInfoFeatureVM>)null, (EquipSlotVM)null)
	{
	}
}
