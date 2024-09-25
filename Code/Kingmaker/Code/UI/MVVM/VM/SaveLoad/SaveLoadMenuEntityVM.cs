using Owlcat.Runtime.UI.SelectionGroup;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveLoadMenuEntityVM : SelectionGroupEntityVM
{
	public readonly SaveLoadMode Mode;

	public SaveLoadMenuEntityVM(SaveLoadMode mode)
		: base(allowSwitchOff: false)
	{
		Mode = mode;
	}

	protected override void DoSelectMe()
	{
	}
}
