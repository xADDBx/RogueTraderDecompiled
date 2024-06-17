using Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;

namespace Kingmaker.Code.UI.MVVM.View.Other.NestedSelectionGroup;

public abstract class NestedSelectionGroupRadioPCView<TEntityVM, TEntityView> : NestedSelectionGroupPCView<NestedSelectionGroupRadioVM<TEntityVM>, TEntityVM, TEntityView> where TEntityVM : NestedSelectionGroupEntityVM where TEntityView : NestedSelectionGroupEntityPCView<TEntityVM>
{
}
