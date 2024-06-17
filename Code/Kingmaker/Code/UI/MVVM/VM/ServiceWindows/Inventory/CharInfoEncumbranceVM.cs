using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class CharInfoEncumbranceVM : CharInfoComponentVM
{
	public EncumbranceVM EncumbranceVm;

	public ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public CharInfoEncumbranceVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		AddDisposable(EncumbranceVm = new EncumbranceVM());
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			if (Unit.Value != null)
			{
				EncumbranceVm.SetCapacity(EncumbranceHelper.GetCarryingCapacity(Unit.Value));
			}
		}));
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		Tooltip.Value = new TooltipTemplateEncumbranceCharacter(Unit.Value);
	}
}
