using Kingmaker.Globalmap.SystemMap;

namespace Kingmaker.Globalmap.Interaction;

public interface IStarSystemInteraction
{
	bool IsVisible();

	bool IsInteractable();

	void Interact(StarSystemObjectEntity entity);
}
