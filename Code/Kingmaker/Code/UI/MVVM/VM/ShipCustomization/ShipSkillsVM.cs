using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.ShipCustomization;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipSkillsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ShipProgressionVM ShipProgressionVM;

	public readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public ShipSkillsVM(ReactiveProperty<BaseUnitEntity> unit, ReactiveProperty<LevelUpManager> levelUpManager, bool isLocked = false)
	{
		IsLocked.Value = isLocked;
		AddDisposable(ShipProgressionVM = new ShipProgressionVM(unit, levelUpManager));
		ShipProgressionVM.CareerPathVM.IsDescriptionShowed.Value = true;
	}

	protected override void DisposeImplementation()
	{
	}
}
