using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;

namespace Kingmaker.GameCommands;

public class PostSaveCallbackCommand : GameCommand
{
	public readonly SaveInfo SaveInfo;

	public readonly SaveCreateDTO DTO;

	public PostSaveCallbackCommand(SaveInfo saveInfo, SaveCreateDTO dto)
	{
		SaveInfo = saveInfo;
		DTO = dto;
	}

	protected override void ExecuteInternal()
	{
		((ISaveManagerPostSaveCallback)Game.Instance.SaveManager).PostSaveCallback(SaveInfo, DTO);
	}
}
