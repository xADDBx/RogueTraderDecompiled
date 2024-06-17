using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Progression.Paths;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;

public class CharInfoSummaryVM : CharInfoComponentVM
{
	public readonly ReactiveProperty<ActionPointsVM> ActionPointVM = new ReactiveProperty<ActionPointsVM>();

	public readonly ReactiveProperty<CareerPathVM> CareerPathVM = new ReactiveProperty<CareerPathVM>();

	public readonly BoolReactiveProperty IsInCombat = new BoolReactiveProperty();

	public readonly CharInfoAlignmentVM CharInfoAlignmentVM;

	public readonly ReactiveProperty<CharInfoStatusEffectsVM> StatusEffects = new ReactiveProperty<CharInfoStatusEffectsVM>();

	public CharInfoSummaryVM(IReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		AddDisposable(CharInfoAlignmentVM = new CharInfoAlignmentVM(unit));
		AddDisposable(StatusEffects.Value = new CharInfoStatusEffectsVM(unit));
	}

	protected override void DisposeImplementation()
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		DisposeAndRemove(ActionPointVM);
		ActionPointsVM disposable = (ActionPointVM.Value = new ActionPointsVM(UnitUIWrapper.MechanicEntity));
		AddDisposable(disposable);
		IsInCombat.Value = UnitUIWrapper.IsInCombat;
		(BlueprintCareerPath, int) tuple = Unit.Value.Progression.AllCareerPaths.LastOrDefault();
		DisposeAndRemove(CareerPathVM);
		if (tuple.Item1 != null)
		{
			AddDisposable(new CareerPathVM(Unit.Value, tuple.Item1, null));
		}
	}
}
