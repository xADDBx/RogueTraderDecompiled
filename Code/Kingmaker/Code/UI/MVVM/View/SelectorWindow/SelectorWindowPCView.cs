using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.View.SelectorWindow;

public class SelectorWindowPCView<TEntityView, TEntityVM> : SelectorWindowBaseView<TEntityView, TEntityVM> where TEntityView : VirtualListElementViewBase<TEntityVM>, IHasTooltipTemplate where TEntityVM : SelectionGroupEntityVM
{
}
