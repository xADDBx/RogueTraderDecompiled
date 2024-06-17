using Kingmaker.EntitySystem.Persistence;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class NewSaveSlotVM : SaveSlotVM
{
	public NewSaveSlotVM(SaveInfo saveInfo, IReadOnlyReactiveProperty<SaveLoadMode> mode, SaveLoadActions actions = default(SaveLoadActions))
		: base(saveInfo, mode, actions, Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad)
	{
	}
}
