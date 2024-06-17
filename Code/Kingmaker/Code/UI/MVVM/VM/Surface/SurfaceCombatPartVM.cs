using System;
using Kingmaker.Code.UI.MVVM.VM.InGameCombat;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat.MomentumAndVeil;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Surface;

public class SurfaceCombatPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly LineOfSightControllerVM LineOfSightControllerVM;

	public readonly CurrentUnitCombatVM CurrentUnitCombatVM;

	public readonly MomentumContextVM MomentumContextVM;

	public readonly UIVisibilityVM UIVisibilityVM;

	public SurfaceCombatPartVM()
	{
		AddDisposable(LineOfSightControllerVM = new LineOfSightControllerVM());
		AddDisposable(CurrentUnitCombatVM = new CurrentUnitCombatVM());
		AddDisposable(MomentumContextVM = new MomentumContextVM());
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
	}

	protected override void DisposeImplementation()
	{
	}
}
