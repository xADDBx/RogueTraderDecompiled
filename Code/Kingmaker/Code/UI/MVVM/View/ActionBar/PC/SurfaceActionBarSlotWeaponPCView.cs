using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarSlotWeaponPCView : SurfaceActionBarSlotWeaponView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, base.TooltipPlace, 0, 0, 0, TooltipPriorityPivots)));
	}
}
