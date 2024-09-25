using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public interface IUILoadService
{
	void Load(SaveInfo saveInfo);
}
