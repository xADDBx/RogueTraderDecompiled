using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Colonization.Base;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyStatPCView : ColonyStatBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}
}
