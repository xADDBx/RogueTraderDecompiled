using Kingmaker.EntitySystem.Persistence.SavesStorage;

namespace Kingmaker.EntitySystem.Persistence;

internal interface ISaveManagerPostSaveCallback
{
	void PostSaveCallback(SaveInfo saveInfo, SaveCreateDTO dto);
}
