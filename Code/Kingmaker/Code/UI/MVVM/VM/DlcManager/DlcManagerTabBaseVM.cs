using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager;

public class DlcManagerTabBaseVM : VMBase
{
	public BoolReactiveProperty IsEnabled { get; } = new BoolReactiveProperty();


	protected DlcManagerTabBaseVM()
	{
		IsEnabled.Value = false;
	}

	protected override void DisposeImplementation()
	{
	}

	public virtual bool SetEnabled(bool value, bool? direction = null)
	{
		IsEnabled.Value = value;
		return true;
	}
}
