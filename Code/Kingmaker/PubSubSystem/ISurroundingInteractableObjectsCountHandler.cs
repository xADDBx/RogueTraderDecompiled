using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;

namespace Kingmaker.PubSubSystem;

public interface ISurroundingInteractableObjectsCountHandler : ISubscriber
{
	void HandleSurroundingInteractableObjectsCountChanged(EntityViewBase entity, bool isInNavigation, bool isChosen);
}
