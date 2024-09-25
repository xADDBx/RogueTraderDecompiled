using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDollCharacterDragUIHandler : ISubscriber
{
	void StartDrag();

	void EndDrag();
}
