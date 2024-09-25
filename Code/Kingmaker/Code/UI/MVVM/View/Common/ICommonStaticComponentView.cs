using Kingmaker.Code.UI.MVVM.VM.Common;

namespace Kingmaker.Code.UI.MVVM.View.Common;

public interface ICommonStaticComponentView
{
	void BindComponent(CommonStaticComponentVM vm);

	void UnbindComponent();
}
