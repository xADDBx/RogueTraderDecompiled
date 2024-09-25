using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.InfoWindow;

public class InfoBodyVM : InfoBaseVM
{
	protected override TooltipTemplateType TemplateType => TooltipTemplateType.Info;

	public InfoBodyVM(TooltipBaseTemplate template)
		: base(template)
	{
	}
}
