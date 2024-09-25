using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISwitchPartyCharactersHandler : ISubscriber
{
	void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2);
}
