using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;

namespace Warhammer.SpaceCombat;

public interface IShieldAbsorbsDamageHandler : ISubscriber<IStarshipEntity>, ISubscriber
{
	void HandleShieldAbsorbsDamage(int before, int after, StarshipSectorShieldsType sector);
}
