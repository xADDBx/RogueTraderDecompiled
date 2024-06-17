using Kingmaker.EntitySystem.Interfaces;

namespace Code.GameCore.Mics;

public interface IPersistentState : InterfaceService
{
	IEntity GetEntityDataFromLoadedAreaState(string uniqueID);
}
