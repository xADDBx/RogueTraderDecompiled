using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IPartyCharacterHoverHandler : ISubscriber
{
	void HandlePartyCharacterHover(BaseUnitEntity unit, bool hover);
}
