using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Warhammer.SpaceCombat;

public interface IUpgradeSystemComponentHandler : ISubscriber<StarshipEntity>, ISubscriber
{
	void HandleSystemComponentUpgrade(SystemComponent.SystemComponentType componentType, SystemComponent.UpgradeResult result);

	void HandleSystemComponentDowngrade(SystemComponent.SystemComponentType componentType, SystemComponent.DowngradeResult result);
}
