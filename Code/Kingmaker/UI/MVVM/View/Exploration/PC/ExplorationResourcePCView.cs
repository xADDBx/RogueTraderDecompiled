using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationResourcePCView : ExplorationResourceBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(this.SetTooltip(new TooltipTemplateSimple(base.ViewModel.Name.Value, base.ViewModel.Description.Value)));
	}
}
