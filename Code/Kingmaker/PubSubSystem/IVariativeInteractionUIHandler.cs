using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface IVariativeInteractionUIHandler : ISubscriber
{
	void HandleInteractionRequest(MapObjectView mapObjectView);
}
