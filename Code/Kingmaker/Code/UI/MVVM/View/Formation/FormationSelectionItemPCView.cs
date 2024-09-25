using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.SelectionGroup.View;

namespace Kingmaker.Code.UI.MVVM.View.Formation;

public class FormationSelectionItemPCView : SelectionGroupEntityView<FormationSelectionItemVM>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		UILog.ViewBinded("FormationSelectionItemPCView");
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		UILog.ViewUnbinded("FormationSelectionItemPCView");
	}
}
