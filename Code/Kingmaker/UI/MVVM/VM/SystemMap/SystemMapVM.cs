using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Common;

namespace Kingmaker.UI.MVVM.VM.SystemMap;

public class SystemMapVM : CommonStaticComponentVM
{
	public readonly StarSystemSpaceBarksHolderVM StarSystemSpaceBarksHolderVM;

	public SystemMapVM()
	{
		AddDisposable(StarSystemSpaceBarksHolderVM = new StarSystemSpaceBarksHolderVM());
	}

	protected override void DisposeImplementation()
	{
	}
}
