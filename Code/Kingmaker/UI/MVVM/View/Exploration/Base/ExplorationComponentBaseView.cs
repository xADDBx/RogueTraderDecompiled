using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationComponentBaseView<T> : ViewBase<T> where T : ExplorationComponentBaseVM
{
	protected override void BindViewImplementation()
	{
	}

	protected override void DestroyViewImplementation()
	{
	}
}
