using Kingmaker.Code.UI.MVVM.VM.Common;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.Common;

public abstract class CommonStaticComponentView<TViewModel> : ViewBase<TViewModel>, ICommonStaticComponentView where TViewModel : CommonStaticComponentVM
{
	public void BindComponent(CommonStaticComponentVM vm)
	{
		Bind(vm as TViewModel);
	}

	public void UnbindComponent()
	{
		Unbind();
	}
}
